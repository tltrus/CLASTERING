
namespace WpfApp
{
    internal class Kmeans
    {
        // Microsoft: Learn / Archive / MSDN / Magazine Issues / 2013 / February 
        // James McCaffrey - "Data Clustering - Detecting Abnormal Data Using k-Means Clustering"
        // https://learn.microsoft.com/en-us/archive/msdn-magazine/2013/february/data-clustering-detecting-abnormal-data-using-k-means-clustering

        public Kmeans()
        {

        }

        public int[] Cluster(double[][] rawData, int numClusters, int seed)
        {
            // k-means++ clustering
            // index of return is tuple ID, cell is cluster ID
            // ex: [2 1 0 0 2 2] means tuple 0 is in cluster 2, tuple 1 in cluster 1,
            //  tuple 2 in cluster 0, tuple 3 in cluster 0, etc.
            double[][] data = Normalized(rawData); // so large values don't dominate

            bool changed = true; // change in at least one cluster assignment?
            bool success = true; // all means computed? (no zero-count clusters)

            double[][] means = InitMeans(numClusters, data, seed); // k-means++ 

            int[] clustering = new int[data.Length]; // all 0

            int maxCount = data.Length * 10; // sanity check
            int ct = 0;
            while (changed == true && success == true && ct < maxCount) // technically this is Lloyd's algorithm
            {
                changed = UpdateClustering(data, clustering, means); // (re)assign tuples to clusters. no effect if fail
                success = UpdateMeans(data, clustering, means); // compute new cluster means if possible. no effect if fail
                ++ct; // k-means typically converges very quickly
            }
            // could check for 0-count clusters here
            return clustering;
        }

        private double[][] InitMeans(int numClusters, double[][] data, int seed)
        {
            // select k data items as initial means using k-means++ mechanism:
            // pick one data item at random as first mean
            // loop k-1 times (remaining means)
            //   compute dist^2 from each item to closest mean
            //   pick a data item w/ large dist^2 as next mean
            // end loop

            double[][] means = MakeMatrix(numClusters, data[0].Length); // result

            List<int> used = new List<int>(); // track which items already means

            // select one data item index at random
            Random rnd = new Random(seed);
            int idx = rnd.Next(0, data.Length); // [0, data.Length-1]
            Array.Copy(data[idx], means[0], data[idx].Length);
            used.Add(idx); // we don't want to select this item again

            for (int k = 1; k < numClusters; ++k) // each remaining mean
            {
                double[] dSquared = new double[data.Length]; // to closest mean
                int newMean = -1; // index of data item to be a new mean
                for (int i = 0; i < data.Length; ++i) // for each data item
                {
                    // if data item i is already a mean, skip the item
                    if (used.Contains(i) == true) continue; // not entirely necessary

                    // compute distances from data[i] to each existing mean (to find closest)
                    double[] distances = new double[k]; // we currently have k means
                    for (int j = 0; j < k; ++j)
                        distances[j] = Distance(data[i], means[k]); // could do dist^2 directly

                    // now get the index of the closest mean
                    int m = MinIndex(distances);
                    // save the associated distance-squared
                    dSquared[i] = distances[m] * distances[m];
                }

                // pick one of the data items, using the squared distances
                // this is a form of roulette wheel selection
                double p = rnd.NextDouble();
                double sum = 0.0; // sum of distances-squared 
                for (int i = 0; i < dSquared.Length; ++i)
                    sum += dSquared[i];
                double cumulative = 0.0; // cumulatiive probability

                int ii = 0; // points into distancesSquared[]
                int sanity = 0; // sanity count
                while (sanity < data.Length * 2) // 'stochastic acceptance'
                {
                    cumulative += dSquared[ii] / sum;
                    if (cumulative >= p && used.Contains(ii) == false)
                    {
                        newMean = ii; // the chosen index
                        used.Add(newMean); // don't pick again
                        break;
                    }
                    ++ii; // next candidate
                    if (ii >= dSquared.Length) ii = 0; // back to first item
                    ++sanity;
                }
                // check if newMean is still -1 . . . 

                // save the data of the chosen index
                Array.Copy(data[newMean], means[k], data[newMean].Length);
            } // k, each mean/cluster

            return means;

        } // InitMeans

        private double[][] Normalized(double[][] rawData)
        {
            // normalize raw data by computing (x - mean) / stddev
            // one alternative is min-max:
            // v' = (v - min) / (max - min)

            // make a copy of input data
            double[][] result = new double[rawData.Length][];
            for (int i = 0; i < rawData.Length; ++i)
            {
                result[i] = new double[rawData[i].Length];
                Array.Copy(rawData[i], result[i], rawData[i].Length);
            }

            for (int j = 0; j < result[0].Length; ++j) // each col
            {
                double colSum = 0.0;
                for (int i = 0; i < result.Length; ++i)
                    colSum += result[i][j];
                double mean = colSum / result.Length;
                double sum = 0.0;
                for (int i = 0; i < result.Length; ++i)
                    sum += (result[i][j] - mean) * (result[i][j] - mean);
                double sd = sum / result.Length;
                for (int i = 0; i < result.Length; ++i)
                    result[i][j] = (result[i][j] - mean) / sd;
            }
            return result;
        }

        private double[][] MakeMatrix(int rows, int cols)
        {
            // convenience matrix allocator for Cluster()
            double[][] result = new double[rows][];
            for (int i = 0; i < rows; ++i)
                result[i] = new double[cols];
            return result;
        }

        private bool UpdateMeans(double[][] data, int[] clustering, double[][] means)
        {
            // returns false if there is a cluster that has no tuples assigned to it
            // parameter means[][] is really a ref parameter

            // check existing cluster counts
            // can omit this check if InitClustering and UpdateClustering
            // both guarantee at least one tuple in each cluster (usually true)
            int numClusters = means.Length;
            int[] clusterCounts = new int[numClusters];
            for (int i = 0; i < data.Length; ++i)
            {
                int cluster = clustering[i];
                ++clusterCounts[cluster];
            }

            for (int k = 0; k < numClusters; ++k)
                if (clusterCounts[k] == 0)
                    return false; // bad clustering. no change to means[][]

            // update, zero-out means so it can be used as scratch matrix 
            for (int k = 0; k < means.Length; ++k)
                for (int j = 0; j < means[k].Length; ++j)
                    means[k][j] = 0.0;

            for (int i = 0; i < data.Length; ++i)
            {
                int cluster = clustering[i];
                for (int j = 0; j < data[i].Length; ++j)
                    means[cluster][j] += data[i][j]; // accumulate sum
            }

            for (int k = 0; k < means.Length; ++k)
                for (int j = 0; j < means[k].Length; ++j)
                    means[k][j] /= clusterCounts[k]; // danger of div by 0
            return true;
        }

        private bool UpdateClustering(double[][] data, int[] clustering, double[][] means)
        {
            // (re)assign each tuple to a cluster (index of closest mean)
            // returns false if no tuple assignments change OR
            // if the reassignment would result in a clustering where
            // one or more clusters have no tuples.

            int numClusters = means.Length;
            bool changed = false;

            int[] newClustering = new int[clustering.Length]; // proposed result
            Array.Copy(clustering, newClustering, clustering.Length);

            double[] distances = new double[numClusters]; // from curr tuple to each mean

            for (int i = 0; i < data.Length; ++i) // walk thru each tuple
            {
                for (int k = 0; k < numClusters; ++k)
                    distances[k] = Distance(data[i], means[k]); // usually Euclidean

                int newClusterID = MinIndex(distances); // find closest mean ID
                                                        //Console.WriteLine("new cluster Id = " + newClusterID);
                                                        //Console.ReadLine();
                if (newClusterID != newClustering[i])
                {
                    changed = true;
                    newClustering[i] = newClusterID; // update
                }
            }

            if (changed == false)
                return false; // no change so bail and don't update clustering[][]

            // check proposed clustering[] cluster counts
            int[] clusterCounts = new int[numClusters];
            for (int i = 0; i < data.Length; ++i)
            {
                int cluster = newClustering[i];
                ++clusterCounts[cluster];
            }

            for (int k = 0; k < numClusters; ++k)
                if (clusterCounts[k] == 0)
                    return false; // bad clustering. no change to clustering[][]

            Array.Copy(newClustering, clustering, newClustering.Length); // update
            return true; // no empty clusters and at least one change
        }

        private double Distance(double[] tuple, double[] mean)
        {
            // Euclidean distance between two vectors for UpdateClustering()
            // consider alternatives such as Manhattan distance
            double sumSquaredDiffs = 0.0;
            for (int j = 0; j < tuple.Length; ++j)
                sumSquaredDiffs += Math.Pow((tuple[j] - mean[j]), 2);
            return Math.Sqrt(sumSquaredDiffs);
        }

        private int MinIndex(double[] distances)
        {
            // index of smallest value in array
            // helper for UpdateClustering()
            int indexOfMin = 0;
            double smallDist = distances[0];
            for (int k = 0; k < distances.Length; ++k)
            {
                if (distances[k] < smallDist)
                {
                    smallDist = distances[k];
                    indexOfMin = k;
                }
            }
            return indexOfMin;
        }

        // for Outlier
        public double[] Outlier_data(double[][] rawData, int[] clustering, int numClusters, int cluster)
        {
            int numAttributes = rawData[0].Length;
            double [] outlier = new double [numAttributes];
            double maxDist = 0.0;
            double[][] means = Allocate(numClusters, numAttributes);
            double[][] centroids = Allocate(numClusters, numAttributes);
            UpdateMeans(rawData, clustering, means);
            UpdateCentroids(rawData, clustering, means, centroids);
            for (int i = 0; i < rawData.Length; ++i)
{
                int c = clustering[i];
                if (c != cluster)
                    continue;
                var dist = Distance(rawData[i], centroids[cluster]);
                if (dist > maxDist)
{
                    maxDist = dist;
                    Array.Copy(rawData[i], outlier, rawData[i].Length);
                }
            }
            return outlier;
        }
        public int Outlier_index(double[][] rawData, int[] clustering, int numClusters, int cluster)
        {
            int numAttributes = rawData[0].Length;
            double[] outlier = new double[numAttributes];
            int outlier_index = 0;
            double maxDist = 0.0;
            double[][] means = Allocate(numClusters, numAttributes);
            double[][] centroids = Allocate(numClusters, numAttributes);
            UpdateMeans(rawData, clustering, means);
            UpdateCentroids(rawData, clustering, means, centroids);
            for (int i = 0; i < rawData.Length; ++i)
            {
                int c = clustering[i];
                if (c != cluster)
                    continue;
                var dist = Distance(rawData[i], centroids[cluster]);
                if (dist > maxDist)
                {
                    maxDist = dist;
                    Array.Copy(rawData[i], outlier, rawData[i].Length);
                    outlier_index = i;
                }
            }
            return outlier_index;
        }

        private double [][] Allocate(int numClusters,int numAttributes)
        {
            double [][] result = new double [numClusters][];
            for ( int k = 0; k < numClusters; ++k)
                result[k] = new double [numAttributes];
            return result;
        }

        private double[] ComputeCentroid (double[][] rawData, int[] clustering, int cluster, double[][] means)
        {
            int numAttributes = means[0].Length;

            double[] centroid = new double[numAttributes];

            double minDist = double.MaxValue;

            for (int i = 0; i < rawData.Length; ++i)
            {
                //Перебираем каждую последовательность данных
                int c = clustering[i];

                if (c != cluster)
                    continue;

                double currDist = Distance(rawData[i], means[cluster]);

                if (currDist < minDist)
                {
                    minDist = currDist;

                    for (int j = 0; j < centroid.Length; ++j)
                        centroid[j] = rawData[i][j];
                }
            }
            return centroid;
        }

        private void UpdateCentroids(double [][] rawData, int [] clustering, double [][] means, double[][] centroids)
        {
            for (int k = 0; k < centroids.Length; ++k)
            {
                double
                [] centroid = ComputeCentroid(rawData, clustering, k, means);

                centroids[k] = centroid;
            }
        }
    }
}
