using UnityEngine;
using System.Collections.Generic;

namespace ConcaveHull {
    public class Init : MonoBehaviour {

        List<Node> dot_list = new List<Node>(); //Used only for the demo

        public string seed;
        public int scaleFactor;
        public int number_of_dots;
        public double concavity;

        public void generateHull() {

        }

        public void setDots(int number_of_dots) {

            dot_list.Clear();
            // This method is only used for the demo!
            System.Random pseudorandom = new System.Random(seed.GetHashCode());
            for (int x = 0; x < number_of_dots; x++) {
                dot_list.Add(new Node(pseudorandom.Next(0, 100), pseudorandom.Next(0, 100), x));
            }
            //Delete nodes that share same position
            for (int pivot_position = 0; pivot_position < dot_list.Count; pivot_position++) {
                for (int position = 0; position < dot_list.Count; position++) {
                    if (dot_list[pivot_position].x == dot_list[position].x && dot_list[pivot_position].y == dot_list[position].y
                        && pivot_position != position) {
                        dot_list.RemoveAt(position);
                        position--;
                    }
                }
            }

            generateHull();

        }

        // Unity demo visualization
        void OnDrawGizmos() {
            setDots(number_of_dots);
            // Convex hull


            // Dots
            Gizmos.color = Color.red;
            for (int i = 0; i < dot_list.Count; i++) {
                Gizmos.DrawSphere(new Vector3((float)dot_list[i].x, (float)dot_list[i].y, 0), 0.5f);
            }
        }
    }

}
