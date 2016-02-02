import sys
import csv
''' Turns dataset into nice feature vectors '''




def parse(filename, outf, cache=None):
    ''' Parses the data file given and separates the X, Y labels.
        Returns X, Y, which are the features for the dataset 
        and the labels.
    '''
    fieldnames = ['age', 'workclass', 'fnlwgt', 'education', 'education-num', 'marital-status', 'occupation', 
                  'relationship', 'race', 'sex', 'capital-gain', 'capital-loss', 'hours-per-week', 'native-country', 'label']
    
    isCached = cache is not None
    if not isCached:
        cache = {}
        for field in fieldnames:
            cache[field] = set()
        
    out = open(outf, 'w')
    
    nlines = 0
    with open(filename, 'r') as csvfile:
        reader = csv.DictReader(csvfile, fieldnames=fieldnames)
        
        for row in reader:
            nlines += 1
            if isCached: continue
            for field in fieldnames:
                cache[field].add(row[field])
        print 'done'
            
        # figure out length
        csvfile.seek(0)
        categorial = {}
        # determine which fields are categorical
        for entry in cache:
            cache[entry] = list(cache[entry])
            if len(cache[entry]) > 50:
                categorial[entry] = False
            else:
                categorial[entry] = True
        
        # go through each, and output the feature vector.
        
        first = True
        count = 0
        for row in reader:
            outline = []
            for entry in row:
                # Bin the categorical variables, write the continuous ones directly
                if entry == 'label': continue
                if not categorial[entry]:
                    pass
                    #outline.append( row[entry] )
                    #count += 1
                else:
                    for i in range(len(cache[entry])):
                        if row[entry] == cache[entry][i]:
                            outline.append("1.0")
                        else:
                            outline.append("0.0")
                        count += 1
            if first:
                first = False
                out.write("{1},{0}\n".format(count, nlines))
                
            out.write("{0},{1}\n".format(','.join(outline), "1" if ">" in row['label'] else "0" ))
            
        
        out.close()
        print "DONE"
        return cache

if __name__ == '__main__':
    cache = parse ("adult_train", "train", None)
    parse ("adult_test", "test", cache)
