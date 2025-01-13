using System.Diagnostics;

namespace ART1
{
    /*
        C# version of the ART1 code from the book:

        M. Tim Jones (Author)
        AI Application Programming (Programming Series) 2nd Edition
        https://www.amazon.com/AI-Application-Programming-Tim-Jones/dp/1584504218

    */
    internal class Program
    {

        const int MAX_ITEMS = 11;
        const int MAX_CUSTOMERS = 10;
        static int TOTAL_PROTOTYPE_VECTORS = 5;

        static double beta = 1.0;         /* Small positive integer */
        static double vigilance = 0.9;    /* 0 <= vigilance < 1 */

        static int numPrototypeVectors = 0;    /* Number of populated prototype vectors */

        static int[][] prototypeVector;

        /* sumVector supports making recommendations. */
        static int[][] sumVector;

        /* Number of occupants of the cluster */
        static int[] members;

        /* Identifies which cluster to which a particular customer belongs */
        static int[] membership;

        /* String names for items in feature vector */
        static string[] itemName;

        /*
         * Feature vectors are contained within the database array.  A one in
         * the field represents a product that the customer has purchased.  A
         * zero represents a product not purchased by the customer.
         */
        static int[][] database;

        static void Main(string[] args)
        {
            Initialize();

            PerformART1();

            DisplayCustomerDatabase();

            for (int customer = 0; customer < MAX_CUSTOMERS; customer++)
            {
                MakeRecommendation(customer);
            }

            Console.ReadKey();
        }

        static void Initialize()
        {
            //prototypeVector = new int [TOTAL_PROTOTYPE_VECTORS][MAX_ITEMS];
            prototypeVector = Tools.CreateJaggedArray<int[][]>(TOTAL_PROTOTYPE_VECTORS, MAX_ITEMS);

            /* sumVector supports making recommendations. */
            //sumVector = new int [TOTAL_PROTOTYPE_VECTORS, MAX_ITEMS];
            sumVector = Tools.CreateJaggedArray<int[][]>(TOTAL_PROTOTYPE_VECTORS, MAX_ITEMS);

            /* Number of occupants of the cluster */
            members = new int [TOTAL_PROTOTYPE_VECTORS];

            /* Identifies which cluster to which a particular customer belongs */
            membership = new int [MAX_CUSTOMERS];

            /* String names for items in feature vector */
            itemName = new string [MAX_ITEMS] {
                    "Hammer", "Paper", "Snickers", "Screwdriver",
                    "Pen", "Kit-Kat", "Wrench", "Pencil",
                    "Heath-Bar", "Tape-Measure", "Binder" };

            /*       Hmr  Ppr  Snk  Scr  Pen  Kkt  Wrn  Pcl  Hth  Tpm  Bdr 
                     (MAX_ITEMS = 11)
             */
            database = new int [MAX_CUSTOMERS][] {
                new int[] { 0,   0,   0,   0,   0,   1,   0,   0,   1,   0,   0},  //   3
                new int[] { 0,   1,   0,   0,   0,   0,   0,   1,   0,   0,   1},  //  2
                new int[] { 0,   0,   0,   1,   0,   0,   1,   0,   0,   1,   0},  // 1
                new int[] { 0,   0,   0,   0,   1,   0,   0,   1,   0,   0,   1},  //  2
                new int[] { 1,   0,   0,   1,   0,   0,   0,   0,   0,   1,   0},  // 1
                new int[] { 0,   0,   0,   0,   1,   0,   0,   0,   0,   0,   1},  //  2
                new int[] { 1,   0,   0,   1,   0,   0,   0,   0,   0,   0,   0},  // 1
                new int[] { 0,   0,   1,   0,   0,   0,   0,   0,   1,   0,   0},  //   3
                new int[] { 0,   0,   0,   0,   1,   0,   0,   1,   0,   0,   0},  //  2
                new int[] { 0,   0,   1,   0,   0,   1,   0,   0,   1,   0,   0}   //   3
            };

            // Initialize example vectors to no membership to any prototype vector
            for (int j = 0; j < MAX_CUSTOMERS; j++)
            {
                membership[j] = -1;
            }
        }

        /*
         *  displayCustomerDatabase( void )
         *
         *  Emit each Prototype Vector with all occupant customer feature vectors.
         *
         */
        static void DisplayCustomerDatabase()
        {
            int customer, item, cluster;

            Console.WriteLine();

            for (cluster = 0; cluster < TOTAL_PROTOTYPE_VECTORS; cluster++)
            {
                Console.Write($"ProtoVector {cluster,2} : ");

                for (item = 0; item < MAX_ITEMS; item++)
                {
                    Console.Write($"{prototypeVector[cluster][item]} ");
                }

                Console.WriteLine("\n");

                for (customer = 0; customer < MAX_CUSTOMERS; customer++)
                {
                    if (membership[customer] == cluster)
                    {
                        Console.Write($"Customer {customer,2}    : ");

                        for (item = 0; item < MAX_ITEMS; item++)
                        {
                            Console.Write($"{database[customer][item]} ");
                        }
                        Console.WriteLine($"  : {membership[customer]} : ");
                    }
                }

                Console.WriteLine();
            }

            Console.WriteLine();
        }

        /*
         *  vectorMagnitude( int *vector )
         *
         *  Compute the magnitude of the vector passed in.  The magnitude is the
         *  number of '1' bits that are set in the vector.  
         *
         */
        static int VectorMagnitude(int[] vector)
        {
            int j, total = 0;

            for (j = 0; j < MAX_ITEMS; j++)
            {
                if (vector[j] == 1) total++;
            }

            return total;
        }

        /*
         *  vectorBitwiseAnd( int *res, int *v, int *w );
         *
         *  Perform a bitwise and of two vectors (resulting in another vector).
         *
         */
        static int[] VectorBitwiseAnd(int[] v, int[] w)
        {
            int[] result = new int[11];
            int i;
            for (i = 0; i < MAX_ITEMS; i++)
            {
                if (v[i] == 1 && w[i] == 1)
                    result[i] = 1; // ПОБИТОВОЕ И
            }
            return result;
        }


        /*
         *  createNewPrototypeVector( int *example )
         *
         *  Create a new prototype vector (new cluster) given the passed example
         *  vector.
         *
         */
        static int CreateNewPrototypeVector(int[] example)
        {
            int i, cluster;

            for (cluster = 0; cluster < TOTAL_PROTOTYPE_VECTORS; cluster++)
            {
                if (members[cluster] == 0) break;
            }

            if (cluster == TOTAL_PROTOTYPE_VECTORS) throw new Exception();
        
        #if DEBUG
            Console.WriteLine("Creating new cluster {0}", cluster);
        #endif

            numPrototypeVectors++;

            for (i = 0; i < MAX_ITEMS; i++)
            {
                prototypeVector[cluster][i] = example[i];
        #if DEBUG
                Console.Write("{0} ", example[i]);
        #endif
            }

                members[cluster] = 1;
        #if DEBUG
            Console.WriteLine();
        #endif
            return cluster;
        }

        /*
         *  updatePrototypeVectors( int cluster )
         *
         *  Recompute the prototype vector for the given cluster passed in.
         *
         */
        static void UpdatePrototypeVectors(int cluster)
        {
            int item, customer, first = 1;

            Debug.Assert(cluster >= 0);
        #if DEBUG
            Console.WriteLine($"Recomputing prototypeVector {cluster} ({members[cluster]})");
        #endif
            for (item = 0; item < MAX_ITEMS; item++)
            {
                prototypeVector[cluster][item] = 0;
                sumVector[cluster][item] = 0;
            }

            for (customer = 0; customer < MAX_CUSTOMERS; customer++)
            {
                if (membership[customer] == cluster)
                {
                    if (first is 1)
                    {
                        for (item = 0; item < MAX_ITEMS; item++)
                        {
                            prototypeVector[cluster][item] = database[customer][item];
                            sumVector[cluster][item] = database[customer][item];
                        }
                        first = 0;
                    }
                    else
                    {
                        for (item = 0; item < MAX_ITEMS; item++)
                        {
                            prototypeVector[cluster][item] &= database[customer][item];
                            sumVector[cluster][item] += database[customer][item];
                        }
                    }
                }
            }
        }

        /*
         *  performART1( void )
         *
         *  Perform the ART1 (Adaptive Resonance Theory) algorithm.
         *
         */
        static int PerformART1()
        {
            int magPE, magP, magE;
            double result, test;
            int done = 0;
            int count = 50;

            while (done == 0)
            {
                done = 1;

                for (int index = 0; index < MAX_CUSTOMERS; index++)
                {
    #if DEBUG
                    Console.WriteLine($"\nExample {index} (currently in {membership[index]})");
    #endif
                    if (index ==9)
                    {

                    }
                    // Step 3
                    for (int pvec = 0; pvec < TOTAL_PROTOTYPE_VECTORS; pvec++)
                    {
                        if (members[pvec] != 0)
                        {
    #if DEBUG
                            Console.WriteLine($"Test vector {pvec} (members {members[pvec]})");
    #endif

                            var andresult = VectorBitwiseAnd(database[index], prototypeVector[pvec]);

                            magPE = VectorMagnitude(andresult);
                            magP = VectorMagnitude(prototypeVector[pvec]);
                            magE = VectorMagnitude(database[index]);

                            result = magPE / (beta + magP);
                            test = magE / (beta + MAX_ITEMS);

    #if DEBUG
                            Console.WriteLine($"step 3 : {result} > {test} ?");
    #endif

                            if (result > test)
                            {
                                // Test for vigilance acceptability
    #if DEBUG
                                Console.WriteLine($"step 4 : testing vigilance {magPE / magE} < {vigilance}");
    #endif

                                if (magPE / magE < vigilance)
                                {
                                    int old;

                                    // Ensure this is a different cluster
                                    if (membership[index] != pvec)
                                    {
                                        old = membership[index];
                                        membership[index] = pvec;

    #if DEBUG
                                        Console.WriteLine($"Moved example {index} from cluster {old} to {pvec}");
    #endif

                                        if (old >= 0)
                                        {
                                            members[old]--;
                                            if (members[old] == 0) numPrototypeVectors--;
                                        }
                                        members[pvec]++;

                                        // Recalculate the prototype vectors for the old and new clusters
                                        if ((old >= 0) && (old < TOTAL_PROTOTYPE_VECTORS))
                                        {
                                            UpdatePrototypeVectors(old);
                                        }

                                        UpdatePrototypeVectors(pvec);

                                        done = 0;
                                        break;
                                    }
                                    else
                                    {
                                        // Already in this cluster
                                    }
                                } // vigilance test
                            }
                        }
                    } // for vector loop

                    // Check to see if the current vector was processed
                    if (membership[index] == -1)
                    {
                        // No prototype vector was found to be close to the example vector. Create a new prototype vector for this example.
                        membership[index] = CreateNewPrototypeVector(database[index]);
                        done = 0;
                    }
                } // customers loop

    #if DEBUG
                Console.WriteLine("\n");
    #endif

                if (--count < 0)
                {
                    break; 
                }
                    
            } // !done

            return 0;
        }

        /*
         *  makeRecommendation( int customer )
         *
         *  Given a customer feature vector and the prototype vector, choose the
         *  item within the vector that the customer feature vector does not have
         *  (is 0) and has the highest sumVector for the cluster.
         *
         */
        static void MakeRecommendation(int customer)
        {
            int bestItem = -1;
            int val = 0;
            int item;

            for (item = 0; item < MAX_ITEMS; item++)
            {
                if ((database[customer][item] == 0) &&
                    (sumVector[membership[customer]][item] > val))
                {
                    bestItem = item;
                    val = sumVector[membership[customer]][item];
                }
            }

            Console.WriteLine($"For Customer {customer}, ");

            if (bestItem >= 0)
            {
                Console.WriteLine($"The best recommendation is {bestItem} ({itemName[bestItem]})");
                Console.WriteLine($"Owned by {sumVector[membership[customer]][bestItem]} out of {members[membership[customer]]} members of this cluster");
            }
            else
            {
                Console.WriteLine("No recommendation can be made.");
            }

            Console.Write("Already owns: ");
            for (item = 0; item < MAX_ITEMS; item++)
            {
                if (database[customer][item] == 1) Console.Write($"{itemName[item]} ");
            }
            Console.WriteLine();
            Console.WriteLine();
        }
    }
}
