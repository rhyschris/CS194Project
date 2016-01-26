#include <iostream>
#include <cmath> 
// Accelerate framework on OS X
// TODO: Add MKL include
#ifdef __APPLE__
#include "Accelerate/Accelerate.h"
#else
#include "cblas.h"
#endif

using namespace std;


/**
 * Calculates scaled softmax on the output matrix.
 */
static void softmax (double *output, double *max_minus, int n){

	// max(output)
	double max = cblas_idamax (n, output, 1);
	// broadcast max to an array
	catlas_dset (n, max, max_minus, 1);
	// output -= max_minus
	catlas_daxpby (n, -1, max_minus, 1, 1, output, 1);
	// output = exp(output)
	vvexp (output, output, (const int *)&n );
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
static void train(double *W, int num_feat, int num_class, double *X, 
				 int *Y, int num_train){
	double loss = 0.0;
	double reg = 1.0;

	double *dW = new double[num_feat * num_class];
	double *output = new double[num_train * num_class];
	//double *max_minus = new double[num_class];

	// create y_hot for each iteration
	int *y_hot = new int[num_class * num_train]; 
	double frac_const = 1.0/num_train;

	// set to 0
	memset (dW, 0, num_feat * num_class * sizeof(double));
	memset (output, 0, num_class * num_train * sizeof(double));
	memset (y_hot, 0, num_class * num_train * sizeof(int));
	
	// set y_hot to 1's in the gold label class.
	for (int i = 0; i < num_train; i++){
		y_hot[i*num_class + Y[i]] = 1;
	}
	// Operation: output = X.dot(W)
	// A --> X:		(num_feat, num_train)
	// B --> W:   		(num_class, num_feat)
	// output --> C: 	(num_class, num_train)
	
	cblas_dgemm (CblasRowMajor, CblasNoTrans, CblasNoTrans,
				num_train, num_class, num_feat, 
				1, X, num_feat, W, num_class, 1, output, num_class);				

	// for (int i = 0; i < num_class; i++){
	// 	for (int j = 0; j < num_train; j++){
	// 		double output_sum = 0.0;
	// 		for (int k = 0; k < num_feat; k++){
	// 			output_sum += X[i*num_train + k] * W[num_class*k + i]; 
	// 		}
	// 		cout << output_sum << endl;
	// 	}
	// }

	for (int i = 0; i < num_class * num_train; i++){
		cout << output[i] << endl;
	}
	
	// 	cout << output[0] << " " << output[1] << endl;
	// 	softmax (output, max_minus, wr);
	// 	cout << output[0] << ", " << output[1] << endl;

	// 	loss -= log (output[ Y[i] ]);
	// 	cout << "loss: " << loss << endl;
	// 	// activation = y_hot - class_prob
	// 	catlas_daxpby (wc, -1.0, output, 1, 
	// 				   1.0, y_hot, 1);

	// 	cout << " activation: " << y_hot[0] << ", " << y_hot[1] << endl;
	// 	// dW -= outer(X[i], activation)
	// 	cblas_dgemm (CblasRowMajor, CblasTrans, CblasNoTrans,
	// 				 wc, wr, wr, -1.0, y_hot, wr, 
	// 				 X + (i*wc), wc, 1, dW, wc);

	// }
	// loss /= num_train;

	// // loss += 0.5 * reg * np.sum(W ** 2)
	// loss += 0.5 * reg * cblas_ddot (wr * wc, W, 1, W, 1);
	// // dW /= num_train
	// cblas_dscal (wr * wc, frac_const, dW, 1);
	// //dW += reg * W
	// cblas_daxpy (wr * wc, reg, W, 1, dW, 1);

	// // print loss to compare against python version.
	// cout << loss << endl;
	// cout << "dW: ";
	// for (int j = 0; j < wr * wc; j++){
	// 	cout << dW[j] << endl;
	// }

	delete (dW);	
	delete (output);
}

int main(int argc, char *argv[]){

	// num_features: 3 
	// num_classes: 2
	// num_examples: 4
	// Row major: X: (num_features x num_examples) :
	// Row major: W: (num_classes x num_features) : 
	// Y: (num_examples,)
	
	double W[6] = {0, 1, 0, 1, 0, 1};
	double X[12] = {2, 3, 4, 2, 3, 4, 2, 3, 4, 2, 3, 4};
	int Y[4] = {1, 0, 0, 1};
	
	int num_feat = 3;
	int num_class = 2;
	int num_train = 4;

	train (W, num_feat, num_class, X, Y, num_train);

	return 0;


}