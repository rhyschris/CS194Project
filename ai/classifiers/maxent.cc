#include <iostream>
#include <cmath> 
#include "maxent.h"
#include "float.h"
#include <random>

// Accelerate framework on OS X
// TODO: Add MKL include
#ifdef __APPLE__
	#include "Accelerate/Accelerate.h"
#else 
#include <cblas.h>
// missing vvlog, vvexp, daxpby from Accelerate
// TODO: discriminate with a namespace
#endif

#define LOWER_TWELVE_MASK 0x00000fff

using namespace std;
 
/* Constructor.  Creates the weight matrices for this given
 * instance.  
 */
Maxent::Maxent(int num_feat, int num_class){
	this->num_feat = num_feat;
	this->num_class = num_class;

	this->W = new double[num_feat * num_class]();
	this->dW = new double[num_feat * num_class]();
}

/* Frees allocated memory before deallocation */
Maxent::~Maxent(){
	delete(this->W);
	delete(this->dW);
}
/* Initializes weights to W, using the LAPACK routine dlaruv. 
 * Params:
 * normal: True if normal distribution, uniform if false
 * scal: The scaling you want compared to a 0-1 distribution 
 */ 
void Maxent::initWeights (bool normal, double scal){

	int dist = normal? 3 : 2;
	// 1) gets you uni(0, 1) (unused)
	// 2) gets you uni(-1, 1)
	// 3) gets you normal (0, 1)
	mt19937 twister(random_device{}());

	// four seed integers between 0 and 4096
	int iseed[4] = {(int)twister() & LOWER_TWELVE_MASK, 
					(int)twister() & LOWER_TWELVE_MASK, 
					(int)twister() & LOWER_TWELVE_MASK, 
					(int)twister() & LOWER_TWELVE_MASK};

	int n = num_feat * num_class;

    dlarnv_(&dist, iseed,  &n, this->W); 
    cblas_dscal (n, scal, this->W, 1);
}
/**
 * Implements a loss function for the softmax classifier.
 * W: (num_feat, num_class)
 * X: (num_train, num_feat) --> each row is a vector
 * Y: (num_train)
 */
double Maxent::train(double *X, int *Y, int num_train,
					int epochs, double alpha, double reg){
	/* Initialize single parameters */

	double frac_const = 1.0/num_train;
	double loss = 0.0;

	int out_sz = num_train * num_class;

	/* Allocating new matrices */
	double *output = new double[out_sz];
	double *max_minus = new double[out_sz];
	double *y_hot = new double[out_sz];
	double *d_sigmoid = new double[out_sz];

	/* Create y_hot */
	memset (y_hot, 0, out_sz * sizeof(double));	
	// set y_hot to 1's in the gold label class.
	for (int i = 0; i < num_train; i++){
		y_hot[i*num_class + Y[i]] = 1;
	}

	for (int iter = 0; iter < epochs; iter++){
		loss = 0.0;
		// set to 0
		memset (dW, 0, num_feat * num_class * sizeof(double));
		memset (output, 0, out_sz * sizeof(double));
		memset (max_minus, 0, out_sz * sizeof(double));

		// make copy for later.
		cblas_dcopy (out_sz, y_hot, 1, d_sigmoid, 1);

		// Operation: output = X.dot(W)
		// A --> X:		(num_feat, num_train)
		// B --> W:   		(num_class, num_feat)
		// output --> C: 	(num_class, num_train)
		// M, N: first dimension is across 

		cblas_dgemm (CblasRowMajor, CblasNoTrans, CblasNoTrans,
					num_train, num_class, num_feat, 
					1, X, num_feat, W, num_class, 1, output, num_class);				

		//	batched up softmax calc.
		for (int i = 0; i < num_train; i++){
			softmax (output + (i*num_class), 
					max_minus + (i*num_class), num_class);
		}
		// outer activation: d_sigmoid = y_hot - output
		cblas_daxpy (out_sz, -1.0, output, 1, 
	 			   d_sigmoid, 1);	
		// elt-wise log of output
		vvlog (output, output, (const int *)&out_sz);

		// loss += 0.5 * reg * np.sum(W ** 2)
		//loss += 0.5 * reg * cblas_ddot (num_feat * num_class, W, 1, W, 1);
		loss -= cblas_ddot (out_sz, output, 1, y_hot, 1);
		loss /= num_train;

		if (iter % 10 == 0)
			cout << "epoch " << iter << ", loss: " << loss << endl;
		// dW -= X.T * d_sigmoid
		cblas_dgemm (CblasRowMajor, CblasTrans, CblasNoTrans,
					 num_feat, num_class, num_train, 
					 1, X, num_feat, d_sigmoid, num_class, -1, dW, num_class);

		// dW / num_train, dW += reg * W
		cblas_dscal (num_class * num_feat, frac_const, dW, 1);
		cblas_daxpy (num_class * num_feat, -reg, W, 1, dW, 1);

		/* W -= alpha * dW */
		catlas_daxpby (num_class * num_feat, alpha, dW, 1, 1, W, 1);
		if (iter % 10 == 0)
			test (X, Y, num_train);	
	}

	delete (output);
	delete (max_minus);
	delete (y_hot);
	delete (d_sigmoid);

	return loss;
}

/**
 * Runs the linear classifier
 * on the test set.
 * 
 * Returns the accuracy.
 */
double Maxent::test(double *X, int *Y, int num_test){
	double *y_pred = new double[num_test * num_class]();


	cblas_dgemm (CblasRowMajor, CblasNoTrans, CblasNoTrans,
			num_test, num_class, num_feat, 
			1, X, num_feat, W, num_class, 1, y_pred, num_class);

	int num_correct = 0;
	for (int i = 0; i < num_test; i++){
		
		int max_ind = 0;
		double cur_max = -DBL_MAX;
		

		for (int j = 0; j < num_class; j++){
			double value = y_pred[i*num_class + j];
			if (value > cur_max) {
				cur_max = value;
				max_ind = j;
			}
		}
		num_correct += (max_ind == Y[i]) ? 1. : 0.;
	}
	double acc = (double)num_correct / num_test;
	cout << "accuracy: " << num_correct << " / " << num_test << " = " 
		 << acc << endl;
	delete(y_pred);
	return acc;
}
/* Private methods
 * -------------------
 */

/**
 * Calculates scaled softmax on the output matrix.
 */
void Maxent::softmax (double *output, double *max_minus, int n){
	//max(output)
	double max = output[ cblas_idamax (n, output, 1) ];
	// broadcast max to an array
	catlas_dset (n, max, max_minus, 1);
	cblas_daxpy (n, -1, max_minus, 1, output, 1);
	
	vvexp (output, output, (const int *)&n);
	// num = sum(output)
	double num = cblas_dasum (n, output, 1);
	
	cblas_dscal (n, 1.0 / num, output, 1);
}