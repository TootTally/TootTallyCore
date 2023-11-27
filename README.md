# TootTallyCore
TootTallyAPI for Trombone Champ Modding

## TootTally Modules
TootTally Modules are mods managed by TootTallyCore. They can be enabled / disabled dynamically without the need of restarting your game. They are use to decouple features into multiple independent components and not over "feature-ise" one single plugin. For more information on how to create a TootTally Modules, head to [the toottally module template page](https://github.com/TootTally/TootTally.ModuleTemplate)

## TootTally Notifs
TootTally Notifications are a convenient way to display information to the user.
- There are 3 values needed when creating a notification: Message, Color, Lifespan (Seconds).
To create a notification, use the TootTallyNotifManager static class and its DisplayNotif method as followed:
```cs
//Full constructor
TootTallyNotifManager.DisplayNotif("MyMessage", Color.white, 6f);

//Simplified constructor (default to white and 6 seconds)
TootTallyNotifManager.DisplayNotif("MyOtherMessage");
```

Just be mindfull that the user probably doesn't want to get spammed with notifs unless necessary :)

## TootTally Animations
TootTally Animations are an easy way to create flexible animations for Unity Gameobject's properties. It uses (Second degree order dynamics)[https://apmonitor.com/pdc/index.php/Main/SecondOrderSystems] to create smooth and alive transitions between two points.
> [!WARNING]
> Keep in mind that these animation are __frame rate dependent__.
> While this makes the animations much smoother at a higher frame rate, there is a risk that the animation looks different when a a long-lasting freeze happens.
Each constants affect the behavior of the animation in 3 different ways: Frequency, Damping, Initial Response.
### Frequency (f)
The amplitude of the animation. Higher frequency usually results in a much faster animation.

### Damping (z)
The deceleration of the animation when getting closer to the destination
- z = 0 no damping -> vibrates forever
- 0 < z < 1 vibrates but end up settling at destination
- z >= 1 takes more time to settle at destination

### Initial Response (r)
The starting velocity of the animation.
- r < 0 "anticipates" the motion (moves in opposite direction for a bit before going to target like a slingshot effect)
- 0 <= r < 1 still takes time to accelerate but less the higher the value
- r = 1 takes no time to accelerate
- r > 1 overshoots destination

It might feel scary at first but once you get the hang of it, your animations will feel a lot more personalized and unique.
- There are 7 different properties that can be animated: TransformScale, TransformPosition, Position (RectTransform), SizeDelta (RT), Scale (RT), EulerAngle, Rotation
- There are 5 parameters needed to create an animation: gameObject, targetVector, timeSpan (seconds), secondDegreeDynamicsAnimation, onFinishCallback (optional)
To create a new animation, use the TootTallyAnimationManager static class and its AddAnimation methods as followed:
```cs
Vector3 targetPosition = new Vector3(5, 10, 15);
SecondDegreeDynamicsAnimation dynamics = new SecondDegreeDynamicsAnimation(1f, 0.95f, 0.5f) //Bounces one time then settles at target, accelerate faster at the start of the animation
TootTallyAnimation myAnimation = TootTallyAnimationManager.AddNewPositionAnimation(myGameObject, targetPosition, 1f, dynamics);
```
Animations can be disposed early if needed by calling the `myAnimation.Dispose()` method.

## TootTally Asset Managers
### PNG
To load your assets into TootTally Asset Manager, simply use the `AssetManager.LoadAssets(path)` method. A convenient way to store your assets is to put them with your .dll file, then use the following line as a path:
```cs
var path = Path.Combine(Path.GetDirectoryName(Instance.Info.Location), "Assets");
AssetManager.LoadAssets(path);
```
You can then access all the assets using the `AssetManager.GetSprite(name)` or `AssetManager.GetTexture(name)` methods.
> [!NOTE]
> Include the extension file to the name such as "MyCoolAsset.png"

### Prefabs
Same as the regular asset manager, but use the `AssetBundleManager.LoadAssets(prefabFilePath)` class instead. Use the `AssetBundleManager.GetPrefab(name)` to get your desired prefab.
> [!NOTE]
> Do not include the .prefab extension. Just enter the name of the prefab such as "MyCoolPrefab"
