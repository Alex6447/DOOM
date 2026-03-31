# DOOM — Player Settings (DOOM-6.8)

Настройки Unity Player Settings для релизной сборки Android:

## Build Settings
- **Build Target:** Android
- **Scripting Backend:** IL2CPP
- **Target Architecture:** ARM64
- **Min API Level:** 26 (Android 8.0)
- **Target API Level:** Auto (latest)

## Player Settings
- **Bundle Identifier:** `com.doom.shooter`
- **Version:** 1.0.0
- **Bundle Version Code:** 1
- **Orientation:** Portrait

## Resolution & Presentation
- **Default Orientation:** Portrait
- **Allow Rotation:** Off

## Other Settings
- **Strip Engine Code:** Enabled
- **Managed Stripping Level:** Medium
- **Minify (Release):** Enabled
- **Internet Access:** Not Required (офлайн-режим)
- **Write Permission:** External (SD Card) — для сохранений

## Canvas Scaler (DOOM-6.5)
На каждом Canvas выставить:
- **UI Scale Mode:** Scale With Screen Size
- **Reference Resolution:** 1080 × 1920
- **Screen Match Mode:** Match Width Or Height
- **Match:** 1.0 (Height — для портрета)

## Проверка разрешений (DOOM-6.6)
| Соотношение | Устройство    | Статус |
|-------------|--------------|--------|
| 16:9        | Стандартный FullHD | ✓ |
| 18:9        | Tall экраны  | ✓ |
| 20:9        | Samsung/Xiaomi| ✓ |
