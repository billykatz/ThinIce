using System.Collections;
using System.Collections.Generic;
using UnityEngine;

     
namespace Extensions
{
    public static class Vector2Extension
    {
        public static bool IsInBounds(this Vector2 vector, int width, int height) {
            return (vector.x >= 0 && vector.x < width) && (vector.y >= 0 && vector.y < height);
        }

        public static List<Vector2> MyNeighbors(this Vector2 origin) {
            List<Vector2> neighbors = new List<Vector2>();

            // orthognal neighbors
            neighbors.Add(new Vector2(origin.x-1, origin.y));
            neighbors.Add(new Vector2(origin.x+1, origin.y));
            neighbors.Add(new Vector2(origin.x, origin.y+1));
            neighbors.Add(new Vector2(origin.x, origin.y-1));

            // diagonal neighbors
            neighbors.Add(new Vector2(origin.x-1, origin.y-1));
            neighbors.Add(new Vector2(origin.x+1, origin.y-1));
            neighbors.Add(new Vector2(origin.x-1, origin.y+1));
            neighbors.Add(new Vector2(origin.x+1, origin.y+1));

            return neighbors;
        }

        public static bool IsNeighbor(this Vector2 vector, Vector2 potentialNeighbor) {
            if (vector.y == potentialNeighbor.y) {
                return (vector.x-1 == potentialNeighbor.x || vector.x+1 == potentialNeighbor.x);        
            }

            if (vector.x == potentialNeighbor.x) {
                return (vector.y-1 == potentialNeighbor.y || vector.y+1 == potentialNeighbor.y);        
            }

            if (vector.x-1 == potentialNeighbor.x && vector.y-1 == potentialNeighbor.y) {
                return true;
            } else if (vector.x+1 == potentialNeighbor.x && vector.y+1 == potentialNeighbor.y) {
                return true;
            } else if (vector.x+1 == potentialNeighbor.x && vector.y-1 == potentialNeighbor.y) {
                return true;
            } else if (vector.x-1 == potentialNeighbor.x && vector.y+1 == potentialNeighbor.y) {
                return true;
            }

            return false;
        }
    }
}

