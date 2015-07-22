# DestructibleGeometry
This Project is a module to destroy convex objects. The project is realized with Unity3D.

<h2>Installation</h2>
To use this project, clone the repository and open this project in Unity.

<h2>Usage</h2>
In the folder <strong>"Assets/Scenes"</strong> is a scene called <strong>"DestructiveTestScene_WithCharacterController.unity"</strong> which is used to demonstrate the functionality of this project

If the script shall run on other objects some requirements must be met:
<ol>
<li>The geometry must be <strong>completely convex</strong>!</li>
<li>The gameobject needs to have a <strong>rigidbody!</strong></li>
<li>A <strong>"Destructor"</strong>-script needs to be added to the gameobject!</li>
<li>A gameobject has to be added to the <strong>"Destructor"</strong>-script, which contains a script of the type <strong>"IUVMapper"</strong>. Example-objects can be found in the folder <strong>"Assets/Scripts/GeometryHelper/Texturing"</strong>.</li>
</ol>

<h2>References</h2>
For the demonstration-scene some textures were taken from other sources:
<ul>
  <li><a href="http://www.123rf.com/photo_6209541_four-different-wool-textures-in-blue-red-green-and-yellow.html">Four different wool textures in blue,red,green and yellow</a> by <a href="http://www.123rf.com/profile_kmiragaya">Karel Miragaya</a></li>
  <li><a href="http://www.highresolutiontextures.com/free-brick-wall-texture-pack">Free brick wall texture pack</a> by <a href="http://www.highresolutiontextures.com/author/admin">Vincent</a></li>
</ul>

