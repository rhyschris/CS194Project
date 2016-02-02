#include <iostream>
#include "maxent.h"
#include <string>
#include <fstream>
/* Test program to run a classifier. */

using namespace std;

/* Reads in data from a file. 
 * The first line is (num_train, num_features - 1) */
void load_data (string filename, double * &X, int * &Y, int & num_train, int & num_feat)
{
  // TODO: implement
  ifstream fin;
  fin.open(filename, ios::in);

  cout << "HERE" << endl;
  fin >> num_train >> num_feat;
  cout <<  num_feat << endl;

  X = new double[num_train * (num_feat+1)](); 
  Y = new int[num_train]();
    
  int xpos = 0;
  for (int i = 0; i < num_train; i++){
    for (int j = 0; j < num_feat; j++){
      fin >> X[xpos];
      xpos++; 
    }
    // bias term
    X[xpos++] = 1.0;
    fin >> Y[i];
  }
  num_feat += 1;

  cout << num_train << " " << num_feat << endl;

} 


int main(int argc, char *argv[])
{ 
  // num_features: 3 
  // num_classes: 2
  // num_examples: 4
  // Row major: X: (num_features x num_examples) :
  // Row major: W: (num_classes x num_features) : 
  // Y: (num_examples,)
  std::cout << "Hello from the main of model.cc" << std::endl;
  
  double *X = NULL;
  double *X_test = NULL;
  int *Y = NULL;
  int *Y_test = NULL;
  /*
  double X[12] = {1, 2, 5, 1, 2, 3, 2, 2, 4, 1, 2, 1};
  int Y[4] = {1, 0, 1, 0};
  */
  int num_train = 0;
  int num_feat = 0;
  int num_class = 2;

  int num_test;
  load_data ("../datasets/adults/train", X, Y, num_train, num_feat);
  load_data ("../datasets/adults/test", X_test, Y_test, num_test, num_feat);  

  Maxent model(num_feat, num_class);
  
  model.initWeights(false, 1e-6);
  model.train (X, Y, num_train, 100, 1.0, 0.001);
  cout << "-----" << " Predict " << " --------- " << endl;
  model.test (X_test, Y_test, num_test);
  
  return 0;
}
