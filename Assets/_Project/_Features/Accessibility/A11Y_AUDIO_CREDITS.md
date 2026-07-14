# Créditos de audio — capa de accesibilidad

Todos los sonidos de esta capa provienen de packs **CC0 (dominio público)** de Kenney (kenney.nl),
verificados en su página el 2026-07-13. CC0 es compatible con CC BY-SA 4.0 (el proyecto base).

| Archivo en `StreamingAssets/A11y/` | Original | Pack | Autor | Licencia |
| --- | --- | --- | --- | --- |
| `door_beep.ogg` | `impactWood_light_001.ogg` (toc en madera = puerta) | Impact Sounds | Kenney (kenney.nl) | CC0 |
| `item_beep.ogg` | `confirmation_001.ogg` (campanita = objeto agarrable) | Interface Sounds | Kenney (kenney.nl) | CC0 |
| `wall_bump_000..004.ogg` | `impactSoft_heavy_000..004.ogg` (golpe sordo = pared) | Impact Sounds | Kenney (kenney.nl) | CC0 |

Historial: la primera iteración usaba tonos abstractos (`bong_001`/`tick_001`); el QA de María Pía pidió
sonidos semánticos ("que la puerta suene a puerta") y se reemplazaron el 2026-07-13.

Fuentes: https://kenney.nl/assets/interface-sounds · https://kenney.nl/assets/impact-sounds

Si un archivo de `Resources/A11y/` falta, el código genera un tono procedural equivalente
(`SonarAudio.CreateBeep`), así que borrar un archivo es una forma segura de volver al beep sintético.

Nota aparte: `Plugins/x86_64/nvdaControllerClient.dll` NO es un asset de audio; es el
NVDA Controller Client oficial de NV Access, licencia LGPL 2.1 (ver
`NVDA_CONTROLLER_CLIENT_LICENSE.txt` en la misma carpeta). Se distribuye sin modificar.
