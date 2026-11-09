# SplintIt VR

Aplicație VR pentru exersarea construirii unei atele improvizate din obiecte obișnuite, în situații simulate.

- [Tema 5 Main Modules — document Word](docs/Tema-5-SplintIt-VR.docx)
- [Tema 5 — sursa editabilă, journey maps și storyboard](project-definition-2.md)
- [Definiția inițială a proiectului și moodboard](project-definition.md)

Tema 5 descrie versiunea ideală a aplicației și planul de producție a asseturilor. Documentul Word conține sublinierile și highlight-ul galben cerute; lista de asseturi este un checklist de producție, cu elementele intenționat nebifate. Diagramele și schițele sunt păstrate în `images/tema-5/` și incluse în document.

Repo-ul conține în prezent documentația de proiectare. Implementarea Unity urmează separat.

## Progres săptămâna 7:

În această iterație am trecut de la documentația de proiectare la o implementare Unity funcțională. Rezumatul modificărilor:

- **Structura proiectului și fișierele de dezvoltare**: Am adăugat scheletul complet al proiectului Unity — scripturile runtime pentru demo (`SplintBuildController`, `SplintItem`, `SplintTypes`, `PlacementZone`, `RoundTimer`, `SaveSystem`, `Feedback`, `NpcReaction`, `Billboard`, `TrackingGuard`, `ComfortSettings`, `DemoFlow`), scriptul de construire a scenei din editor (`HikingDemoSceneBuilder`), assembly definitions, fișierele `.gitignore`, documentație de setup (`SETUP.md`, `OPEN_IN_UNITY.md`) și scripturi auxiliare de scaffolding.

- **Configurarea Unity XR și rezolvarea erorilor Oculus**: Am adăugat setările complete de proiect Unity (Project/Graphics/Quality/Input settings), configurarea OpenXR și XR Interaction Toolkit (loadere, layere de interacțiune, simulator de dispozitiv XR), pipeline-ul de randare universal (URP) și manifestul de pachete. Am reorganizat scripturile în structura standard `Assets/` și am generat fișierele `.meta` necesare.

- **Corectarea poziției camerei XR**: Am rezolvat problema în care camera pornea sub nivelul solului. `HikingDemoSceneBuilder` setează acum modul de tracking „Floor" pentru XR Origin și aplică un offset de ~1.6 m în editor/simulator. `ComfortSettings` captează înălțimea de bază la pornire și aplică offset-urile de confort (Seated/Height) peste aceasta, astfel încât comutarea setărilor să nu mai coboare camera sub hartă.

yt link:https://youtu.be/X7a8WIioqv4