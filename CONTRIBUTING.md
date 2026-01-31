# Contributing

## Contributing workflow

1. Create a new branch from `dev` for your pull request
2. Test your changes thoroughly
3. Submit a pull request to the `dev` branch

## Branch strategy

- `main` - Stable releases
- `dev` - Active development (base your work/forks on this)
- `branch/feat/feature-name` - New features
- `branch/fix/bug-description` - Bug fixes
- `branch/refac/component-name` - Code rewrites

## Map generation
* [A simplified summary of SCP - Containment Breach's map generation](https://docs.google.com/document/d/1HoehyQrJDPF8rQ0Zo-utYM8xkr82wkwDrJ0TW-KaiC4/edit?tab=t.0#heading=h.gi8p2nr14rh7)
* [Documentation for SCP - Containment Breach: Unity's map generation](https://docs.google.com/document/d/1Jc8Rn0uu-94lyZqKbLOny1LYTI5gm28u4jWcFTT9ltI/edit?tab=t.0#heading=h.gi8p2nr14rh7)

## Font generation
* Majority of fonts - Use Extended ASCII for your Character Set
* Courier New font - Use Unicode Range (Hex), and use this range `20-7E,A0-FF,400-4FF`
* `SDFAA_HINTED` is recommended for Render Mode

## Code conventions

### Method Order

All scripts inheriting from MonoBehaviour should be in this order:

```csharp
Awake()
Start()
OnEnable()
OnDisable()
OnDestroy()
OnValidate()
Reset()
AssignReferences() // for Reset

Update()
FixedUpdate()
LateUpdate()

OnCollisionEnter()
OnCollisionStay()
OnCollisionExit()
OnTriggerEnter()
OnTriggerStay()
OnTriggerExit()

OnDrawGizmos()
OnDrawGizmosSelected()
```

### Naming Convention

- **Classes**: PascalCase (`PlayerController`)
- **Interfaces**: PascalCase with I prefix (`IInteractable`)
- **Constants**: UPPER_SNAKE_CASE (`MAX_INVENTORY_SIZE`)
- **Methods**: PascalCase (`MovePlayer()`)
- **Public fields**: PascalCase (`MaxHealth`)
- **Private serialized fields**: camelCase (`walkSpeed`)
- **Private fields**: camelCase with underscore prefix (`_currentSpeed`)
- **Properties**: PascalCase (`IsGrounded`)

### Code Quality

* Keep methods focused
* DO NOT USE SHJITY coroutines and use UniTask instead
* PrimeTween, make sure your tweens use both `ToYieldInstruction()` and `ToUniTask()`
* Use meaningful variable names
* Remove debug code before committing
* Avoid any hardcoded values, use SerializeField or constants

## Unity Guidelines

### Prefabs

* Keep prefabs modular
* Test prefab changes

### Asset Organization

* Follow the existing folder structure since I worked very hard to keep it nice :(
* Remove unused assets

### Unity Version

* Ensure you're using the Unity version specified in the project
* Check `ProjectSettings/ProjectVersion.txt` for the required version

## Pull Request Process

1. Ensure your code follows all conventions
2. Test your changes thoroughly
3. Update documentation
4. Create a PR targeting the `dev` branch
5. Provide a clear description of what your PR does
6. Reference any related issues

## Testing Requirements

Before submitting a pull request:

* Test all affected gameplay mechanics
* Verify no console errors or warnings
* Test in both the Editor and Build (if possible)
* Check for performance issues
* Ensure no conflicts with existing features

## Questions?

If you have questions about contributing, join the [Discord](https://discord.gg/dVdY4PuGfp). :)