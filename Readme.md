# High Performance Raytracing 
## Disclaimer
Diese Beschreibung des Projektes ist lediglich oberflächlich - die genaue Funktionsweise kann dem Code entnommen werden. Außerdem wird vorrausgesetzt, 
dass der Leser mit "Octrees" und ihrer Implementierung sowie mit Grafikprogrammierung, insbesondere Shadern und C# vertraut sind. <br> 
## CPU seitig
Diese Beispielanwendung zeigt die Implementierung eines Shader-basierten Raytracing Test-Frameworks. 
Die Implementierung erzeugt rekursiv einen Octree über C# Klassen, die jeweils einen Knoten des Baums darstellen. Jeder dieser Knoten erhält einen Dichtewert - 
liegt dieser über einem Schwellwert, so stellt dieser Knoten einen Voxel dar. Jeder dieser Knoten kann in 4 bytes dargestellt werden: <br> <br> 
- Zunächst beschreiben zwei Bytes, an welchem Index in einer 1D-Repräsentation des Octrees (also einer einfachen Liste aller Knoten) die Child-Knoten eines gegebenen Knotens beginnen. 
Die maximale Größe der Liste von Knoten ist somit 2^16 oder 65,536 Knoten oder eine maximale Tiefe von 5.
Die Liste von allen Knoten des Baums werden so geordnet, dass alle Child-Knoten eines Knotens direkt aufeinander folgen. Dies ermöglicht einfache Iteration des Baums im Shader.
- Darauf folgen 15 Bits, die die 3 dimensionalen Koordinaten des Knotens relativ zum Ursprung des Baums festlegen. 
- Zuletzt markiert ein Bit, ob der letzte "Geschwisterknoten", also der letzte Knoten der Child-Knoten, erreicht wurde. <br>
Das verwendete Layout lässt sich also wie folgt darstellen: <br> 
[childStartIndex; X;Y;Z; isLastChild (default:0, can be set manually)]
<br> <br> 
## Shader-seitig
Im Vertex-Shader werden die Strahlrichtung und der Ursprung generiert, sodass der Fragment-Shader mit diesen weiterarbeiten kann. 
Für jeden Pixel wird nun ein Strahl ausgesandt, der gegen den Octree auf Überschneidung überprüft wird. Die Treffer werden nach Entfernung vom Ursprung geordnet übernommen - der nächste Treffer bestimmt die Farbe des Pixels.
Aufgrund der schnellen Skalierung der Zugriffsgeschwindigkeit auf Elemente im Octree von O(log N) sind so eine große Anzahl an Voxeln ohne große zusätzliche Geschwindigkeitseinbußen möglich. 
Der Shader nimmt die 1D-Repräsentation des Octrees über einen HLSL ´Buffer´ entgegen und iteriert ohne Rekursion über den Baum. Dabei kommt dieser mit einer sehr geringen VRAM-Belastung aus, da lediglich ein Stapel der Tiefe des Baumes verwendet wird, um die Tiefe der aktuellen Knoten zu speichern.
Im Detail beginnt der Algorithmus an der Wurzel des Baums und überprüft die Überschneidung mittels Raytracing. Ist die Überschneidung gegeben, erhöht der Algorithmus die Tiefe um 1 und iteriert über die Child-Knoten. Dies wird solange wiederholt, bis ein Blatt erreicht wurde. 
Dies stellt einen Treffer dar. Mit Treffer wird wie bereits beschrieben verfahren.<br> <br> 
## Fazit
Letztendlich ist diese Anwendung im Rahmen eines reinen Interessen-Projekts entstanden und soll lediglich der Implementierung und Erkundung der Octrees dienen. 
Insbesondere der nicht-rekursive Shaderalgorithmus, der den beschränkten API-Ressourcen von Grafikkarten geschuldet ist, stellt meinen persönlichen Favoriten dieses Projekts dar.
