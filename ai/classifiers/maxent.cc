#include <iostream>
#include <cmath> 
// Accelerate framework on OS X
// TODO: Add MKL include
#include "Accelerate/Accelerate.h"

using namespace std;


/**
 * Calculates scaled softmax on the output matrix.
 */
static void softmax (double *output, int n){

	// max(output)
	double max = cblas_idamax (n, output, 1);
	double num = 0.0; // Try dscal
	for (int i = 0; i < n; ++i) {
		output[i] = exp( output[i] - max);
		num += output[i];  
	}
	cblas_dscal (n, 1.0 / num, output, 1);
}


/**
 * Implements a loss function for 
 *
 *
 *
 */
static void train(double *W, int wx, int wy, double *X, 
				 int *Y, int num_train, int num_feat,
				 int num_classes){

	double loss = 0.0;
	double reg = 1.0;

	double *dW = new double[wx * wy];
	memset (dW, 0, wx * wy * sizeof(double));
	double *output = new double[num_classes];
	double *y_hot = new double[num_classes]; 

	double frac_const = 1.0/num_train;

	for (int i = 0; i < num_train; i++){
		memset (output, 0, num_classes * sizeof(output));
		memset (y_hot, 0, num_classes);
		y_hot[Y[i]] = 1.;
		// output = Wx
		cblas_dgemv (CblasRowMajor, CblasNoTrans, wx, wy, 
					1, W, wx, X + (i*num_feat), 
					1, num_classes, output, 1);		

		softmax (output, num_classes);

		loss -= log (output[ Y[i] ]);
		// activation = y_hot - class_prob
		catlas_daxpby (num_classes, -1.0 * frac_const, output, 1, 
					   frac_const, y_hot, 1);

		// dW -= y_hot * transpose(X)
		// TODO: check integrity of dgemm
		cblas_dgemm (CblasRowMajor, CblasNoTrans, CblasTrans,
					 wx, wy, 1, -1.0, y_hot, num_classes, 
					 X + (i*num_feat), num_feat, 1, dW, wx);
	}

	// loss += 0.5 * reg * np.sum(W ** 2)
	
	loss += (0.5 * reg * cblas_ddot (wx * wy, W, 1, W, 1));
	// dW /= num_train
	cblas_dscal (wx * wy, frac_const, dW, 1);

	//dW += reg * W
	catlas_daxpby (wx * wy, reg, W, 1, 1, dW, 1);
	
	delete (dW);
	delete (output);
}

int main(int argc, char *argv[]){

	cout << "Hello world" << endl;
	double W[4] = {0., 1., 2., 3};
	double X[6] = {2.0, 3.0, 4.0, 5.0, 6.0, 7.0};
	int Y[6] = {0, 1, 1, 0, 1, 0};
	
	int wx = 2;
	int wy = 2;
	int num_train = 3;
	int num_feat = 2;
	int num_classes = 2;

	train (W, wx, wy, X, Y, num_train, num_feat, num_classes);

	return 0;


}