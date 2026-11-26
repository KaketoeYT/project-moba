# Project MOBA

Unity MOBA prototype.

## Layout

```
Assets/_Project/
  Art/            materials, textures, models
  Data/           ScriptableObject assets (Abilities, Characters)
  Environment/    terrain and level geometry data
  Prefabs/        prefabs grouped by type
  Scenes/         scenes
  Scripts/        runtime code grouped by feature
Docs/             design documents (characters, mechanics)
```

Third-party and asset store content goes in `Assets/ThirdParty/`, outside `_Project`.

## Conventions

- Folders and files use PascalCase; a script's filename matches its class name.
- Move and rename assets inside the Unity editor, or with `git mv` together with the `.meta` file.
- Design documents live in `Docs/`, never inside `Assets/`.
