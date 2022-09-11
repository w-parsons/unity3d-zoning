# Area Designation / Zoning in Unity3D

![](https://github.com/w-parsons/unity3d-zoning/blob/main/media/ingame.gif)

## About
This project shows a method of designating unique zones in a Unity3D project, such as rooms. It was inspired by games such as [Prison Architect](https://prisonarchitect.paradoxwikis.com/Room), where the player designates areas in the in game map for different tasks.

This approach uses a [union find data structure](https://en.wikipedia.org/wiki/Disjoint-set_data_structure) to group areas that make contact with each other. Each distinct area is uniquely identifiable (shown in the sample project by a unique colour), and points can be tested to see which (if any) area they are contained by.

Features include
- Drawing zones
- Deleting zones
- Preventing overlap of zones
- Zone touch/overlap detection based purely on position (no pesky colliders or physics required)
- Preventing zones being treated as touching when only their corners are contacting each other
- Counting total area filled by zones
- Counting distinct zones

## Resources
- https://en.wikipedia.org/wiki/Disjoint-set_data_structure
- https://unity3d.college/2017/10/08/simple-unity3d-snap-grid-system/

## Screenshots
![](https://github.com/w-parsons/unity3d-zoning/blob/main/media/sampleproject.gif)
![](https://github.com/w-parsons/unity3d-zoning/blob/main/media/ingamescreenshot.png)
![](https://github.com/w-parsons/unity3d-zoning/blob/main/media/screen1.png)
![](https://github.com/w-parsons/unity3d-zoning/blob/main/media/screen2.png)
