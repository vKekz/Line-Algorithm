# Line Algorithms
This is a Unity test project containing two main algorithms for splitting and merging lines. Lines can be placed on a horizontally generated grid which is made out of spheres. 

**Hold the left mouse button and release at any given point to draw a line:**

![Screenshot 2024-02-06 221200.png](Assets%2FImages%2FLine.png)

*You can see the line's index from the internal list, it's length in meters and the direction it was drawn to*.

**Drawing another line across the current line will trigger the split algorithm:**

![Intersection.png](Assets%2FImages%2FIntersection.png)

*As you can see the algorithm found four ways of dividing the two intersecting lines. Marked as red is the intersection.*

**Drawing one more line across the first line (left):**

![Other intersection.png](Assets%2FImages%2FOther%20intersection.png)

**Expanding line no. 6 (top-left):**

![Merge.png](Assets%2FImages%2FMerge.png)

*I started from the end of previously line no. 6 and the second algorithm merged the lines together.*

**Some information at the end:**
- Bugs or special cases might occur
- Example usage for these algorithms: Wall generation and manipulation