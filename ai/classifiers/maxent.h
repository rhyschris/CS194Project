#ifndef _maxent_
#define _maxent_

class Maxent {

 public: 
  /**
   * Runs training on the maxent classifier.  
   * Params: 
   * X: row-major (num_examples, num_features)
   * matrix concatenated into one array
   * Y: row-major (num_examples,) array, with labels
   * corresponding to the index of the gold label
   * 
   * Return: the loss function
   */
  double train(double *X, int *Y, int num_train, 
              int epochs, double alpha, double reg);
  
  /**
   * Same as train. 
   */
  double test(double *X, int *Y, int num_test);

  void initWeights(bool normal, double scal);

  Maxent(int num_feat, int num_class);
  ~Maxent();

 private:
  void softmax (double *output, double *max_minus, int n);
  /* Weight matrices */
  double *W;
  double *dW;

  /* Matrix dimensions */
  int num_feat;
  int num_class;

};

#endif
