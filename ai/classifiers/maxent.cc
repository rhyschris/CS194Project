#include <iostream>
#include <cmath> 
// Accelerate framework on OS X
// TODO: Add MKL include
#ifdef __APPLE__
	#include "Accelerate/Accelerate.h"
#else 
#include <cblas.h>
// missing vvlog, vvexp, daxpby from Accelerate
// TODO: discriminate with a namespace
#endif

using namespace std;


/**
 * Calculates scaled softmax on the output matrix.
 */
static void softmax (double *output, double *max_minus, int n){
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


/**
 * Implements a loss function for the softmax classifier.
 * W: (num_feat, num_class)
 * X: (num_train, num_feat) --> each row is a vector
 * Y: (num_train)
 */
static void train(double *W, double *X, int *Y, 
				  int num_feat, int num_class, int num_train){
	/* Initialize single parameters */
	double reg = 1e-3;
	double frac_const = 1.0/num_train;

	int out_sz = num_train * num_class;
	int epochs = 100;
	double alpha = 0.01;

	/* Allocating new matrices */
	double *dW = new double[num_feat * num_class];
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
		double loss = 0.0;
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

		loss -= cblas_ddot (out_sz, output, 1, y_hot, 1);
		loss /= num_train;
		// loss += 0.5 * reg * np.sum(W ** 2)
		loss += 0.5 * reg * cblas_ddot (num_feat * num_class, W, 1, W, 1);
		cout << "loss: " << loss << endl;

		// dW -= X.T * d_sigmoid
		cblas_dgemm (CblasRowMajor, CblasTrans, CblasNoTrans,
					 num_feat, num_class, num_train, 
					 1, X, num_feat, d_sigmoid, num_class, -1, dW, num_class);

		// dW / num_train, dW += reg * W
		cblas_dscal (num_class * num_feat, frac_const, dW, 1);
		cblas_daxpy (num_class * num_feat, -reg, W, 1, dW, 1);

		/* W -= alpha * dW */
		catlas_daxpby (num_class * num_feat, alpha, dW, 1, 1, W, 1);
	}


	delete (dW);	
	delete (output);
	delete (max_minus);
	delete (y_hot);
	delete (d_sigmoid);
}

int main(int argc, char *argv[]){

	// num_features: 3 
	// num_classes: 2
	// num_examples: 4
	// Row major: X: (num_features x num_examples) :
	// Row major: W: (num_classes x num_features) : 
	// Y: (num_examples,)
	
	double W[6] = {1, 2, 3, 4, 5, 6};
	double X[12] = {2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13};
	int Y[4] = {1, 0, 0, 1};
	
	int num_feat = 3;
	int num_class = 2;
	int num_train = 4;

	train (W, X, Y, num_feat, num_class, num_train);

	return 0;

}