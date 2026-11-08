# Unity 2D Performance Optimization
Performance optimization and lag reduction for my 2D Unity game, [`Blackpeak Depths`](https://youtu.be/lfduegtb_oA?si=fiB57OzAbNfddShL).

A technical demonstration showcasing how to optimize a dense 2D scene in Unity (handling thousands of active items) by replacing heavy per-frame updates and global physics calculations with a **distance-based activation system** and optimized interaction logic.

---

##  Demo
| Before Optimization ~20 FPS | After Optimization ~50 FPS |
| :---: | :---: |
| <img src="Assets/Images/20_fps.gif" width="350">  |  <img src="Assets/Images/50_fps.gif" width="350">|

---

## Project Goals
* Identify and diagnose performance bottlenecks using the **Unity Profiler**.
* Eliminate unnecessary CPU overhead caused by constant script execution and global 2D physics simulations.
* Implement a lightweight **distance-based culling strategy** to only enable physics and logic when items are close to the player.
* Optimize per-item pickup scripts to prevent garbage collection and redundant checks.

---

## The Problem (Profiling & Diagnosis)
When spawning over 1,000 active 2D items simultaneously, the game suffered from severe frame drops.

* **Profiler Insights:** 
  * The `PlayerLoop` was heavily congested by individual item scripts running checks every frame.
  * `Physics2D.Simulate` was overworked trying to compute collisions for thousands of static/inactive items.

![profiler_image](Assets/Images/cpu_overhead_profiler.png)

---

## The Solutions

### 1. Distance-Based Activation (Dynamic Physics & Logic Culling)
Instead of letting every item run its logic and physics continuously, items remain dormant/disabled by default. Each item checks the distance to the player. 
* When the player gets close $\rightarrow$ Enable `Rigidbody2D` / physics and interaction logic.
* When the player moves away $\rightarrow$ Disable components to save CPU cycles.

* **Key script:** [`PickupItem.cs`](Assets/Scripts/PickupItem.cs)
* **Snippet example:**
```csharp
// if the player is near the item
if (currentDistance < maxDistance)
{
    // we reactivate the physics
    rb.bodyType = RigidbodyType2D.Dynamic;
    rb.constraints = RigidbodyConstraints2D.None;
}
else if (rb.bodyType != RigidbodyType2D.Kinematic && rb.linearVelocity.magnitude < 1f)
{
    // we freeze the object
    rb.bodyType = RigidbodyType2D.Kinematic;
    rb.constraints = RigidbodyConstraints2D.FreezeAll;
}
```

---

## Watch the Video
You can watch a devlog/video I made about it (in French):

[<img src="Assets/Images/optimization_video_thumbnail.png" width="400">](https://www.youtube.com/watch?v=fclpj6KCyAs)
