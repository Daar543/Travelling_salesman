# Travelling_salesman
An exact solution for Travelling salesman problem on a complete graph

A travelling salesman problem (TSP) requires the "salesman" to visit all "cities" and return to its original one. 
In a graph theory, this means finding a path through all vertices, and this path is also assumed to be Hamiltonian - not repeating any vertices.

This problem was proven to be NP-hard, and such instances of this problem are often approximated. 

However, this solution aims to be exact in all situations (given complete graph), and is asymptotically slow (O(n^2 * 2^n)) - 
yet faster than enumerating all paths (O(n!)).
Instead of enumerating all paths, all subsets of the vertices are considered (this is the 2^n) with specified entry and exit vertex (this is the n^2). 
Starting from smallest sets, the algorithm finds a best way to extend a shortest way through k vertices to a shortest way through k+1 vertices - this is called dynamic programming.
