#                                                                                                                                                                                                                          
# Based on Makefiles previously written by Julie Zelenski.                                                                                                                                                                 
#                                                                                                                                                                                                                          
CXX = clang++
# The CXXFLAGS variable sets compile flags for gcc:                                                                                                                                                                        
#  -g                         compile with debug information                                                                                                                                                               
#  -Wall                      give all diagnostic warnings                                                                                                                                                                  
#  -pedantic                  require compliance with ANSI standard                                                                                                                                                         
#  -O0                        do not optimize generated code                                                                                                                                                                
#  -std=c++0x                 go with the c++0x extensions for thread support, unordered maps, etc                                                                                                                          

CXXFLAGS = -g -Wall -pedantic -O0 -std=c++0x

# The LDFLAGS variable sets flags for linker                                                                                                                                                                                
#  -lm       link in libm (math library)                                                                                                                                                                                    
#  -framework Accelerate: Accelerate for vector linear algebra.
#  -lopenblas
LDFLAGS = -lm -lpthread -framework Accelerate

# In this section, you list the files that are part of the project.                                                                                                                                                         
# If you add/change names of header/source files, here is where you                                                                                                                                                         
# edit the Makefile.                                                                                                                                                                                                        
SOURCES = \
        maxent.cc \
	model.cc \
        #news-aggregator-utils.cc \
        stream-tokenizer.cc \
        rss-feed.cc \
        rss-feed-list.cc \
        html-document.cc \
        rss-index.cc
HEADERS = \
	maxent.h \
        #news-aggregator-utils.h \
        stream-tokenizer.h \
        rss-feed.h \
        rss-feed-exception.h \
        rss-feed-list.h \
        rss-feed-list-exception.h \
        html-document.h \
        html-document-exception.h \
        rss-index.h

OBJECTS = $(SOURCES:.cc=.o)
TARGETS = classifier

default: $(TARGETS)

classifier: maxent.o model.o
	$(CXX) $(CXXFLAGS) -o $@ $^ $(LDFLAGS)

#news-aggregator.o rss-feed.o rss-feed-list.o rss-index.o html-document.o stream-tokenizer.o news-aggregator-utils.o
# In make's default rules, a .o automatically depends on its .c file                                                                                                                                                                       # (so editing the .c will cause recompilation into its .o file).                                                                                                                                                                            
# The line below creates additional dependencies, most notably that it                                                                                                                                                                     
# will cause the .c to reocmpiled if any included .h file changes.                                                                                                                                                                        
Makefile.dependencies:: $(SOURCES) $(HEADERS)
	$(CXX) $(CXXFLAGS) -MM $(SOURCES) > Makefile.dependencies -include Makefile.dependencies

# Phony means not a "real" target, it doesn't build anything                                                                                                                                                                               
# The phony target "clean" that is used to remove all compiled object files.                                                                                                                                                                
.PHONY: clean spartan

clean:
	@rm -f $(TARGETS) $(OBJECTS) core Makefile.dependencies 

spartan: clean
	@rm -f *~ .*~
