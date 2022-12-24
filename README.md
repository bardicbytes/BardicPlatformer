# The **BardicBytes Framework** for Unity Development
## Actors
The purpose of the actors system is to centralize and standardize connectivity between GameObjects and MonoBehaviours inside of and between Scenes.

### Actor
The **Actor** beahviour is the fundamental component to the system. A GameObject generally only begins to be relevant to this framework if it has an Actor component.

Actor will automatically cache refrences to all Components, including MonoBehaviours and ActorModules.

**Actor.HasModule<T>()** should be used before **Actor.GetModule<T>()** or **Actor.GetModules<T>()**, but GetModule() will return null. 

All actors automatically register with a static **Director** Behaviour to handle Unity's **Update()** message in a single MonoBehaviour. The update is propgated from Director through Actor and to every ActorModule. 

**Actor.SelfDestruct()** is the proper way to destroy an Actor.

### ActorModule
Override **ActorModule.ActorUpdate()** or **ActorModule.ActorFixedUpdate()** to take advantage of the Actor system's more efficient Update feature.

**ActorModule.GetModule<T>()** and **ActorModule.GetModules<T>()** are a convenience method for Actor's methods of the same signature.

## EventVars
This is taking advantage of Unity's serialization and ScriptableObjects to allow for lightweight messaging between objects that otherwise have no reference to eachother.

Derrive from **BaseGenericeventVar** or **EvaluatingEventVar** to create new Event Vars for any data type.

### EventVar
**EventVar.HasValue**
**EventVar.Value** will evaluate and return
**EventVar.Raise()**
**EventVar.Raise(InT data)**

### EventVarListener
This component is an easy way to subscribe to an EventVar without writing code.

### EventVarInstancer
Creates runtime instances of data-laden EventVars.
### EventVarInstanceField

## Tags
### TagModule
This implementation of ActorModule has an array of ActorTags with which it registers.

### ActorTag
**ActorTag**s are an implementation of **BaseGenericEventVar**. Any Script with an ActorTag reference can iterate over every Actor "tagged" with this asset.


## Effects
An Eventvar based solution for playing audio and visual effects. Play an effect anywhere anytime with an asset reference.

## More Features
### ScriptableObjectCollection

### Pool
A minimalistic Prefab pooling system that doesn't require any setup.

### BardicBuilder
A ScriptableObject based solution for managing multiple build processes for different platforms and defines.
