# Skill-Issue
Juego FPS 1v1 de acción frenética


# Organización del proyecto

	GitHub
• Para subir carpetas vacías a un sistema de gestión de versiones basado en Git, se puede crear un fichero vacío .gitKeep en dicha carpeta 

• También hay que asegurarse de que se suben los ficheros .meta al repositorio, ya que contienen información importante sobre ficheros y directorios del juego, como por ejemplo, la configuración de importación de las texturas

	Escena
• Cuando varias personas trabajan a la vez sobre la misma escena, el sistema de control de versiones puede tener problemas a la hora de integrar los cambios

• Es mejor dividir los niveles en varias escenas más pequeñas para reducir el riesgo de conflictos

• En tiempo de ejecución, el proyecto puede cargar escenas de forma aditiva:
	-SceneManager.LoadScene(int sceneBuildIndex, LoadSceneMode.Additive)	
	
• También se puede lanzar la carga en segundo plano:
	-SceneManager.LoadSceneAsync(int sceneBuildIndex, LoadSceneMode.Additive)

	Objetos
• Recuerda usar prefabs siempre que sea posible

• Organiza la escena para evitar que la jerarquía crezca demasiado

• Usa gameobjects vacíos para crear dicha organización

• Al crear objetos en tiempo de ejecución, aparecen en la raíz del grafo de escena. Si se crean muchos objetos, puede pasar que la escena se vuelva inmanejable.
	-Para manejarlo correctamente, al instanciar un objeto, se le puede indicar un padre(asegúrate que su Transform sea la identidad). Así se puede desplegar o replegar el padre para ver los objetos.
	
	
	Otros consejos
• Always playable: que el proyecto siempre sea jugable.

• Subir código a menudo

• Prueba el juego tras la actualización y verifica que funciona

• Cuidado al subir sólo parte de los cambios, o no subir ficheros nuevos

•En el mensaje de commit, explicar brevemente el objetivo de los cambios realizados


	Carpetas
• Cada fichero y carpeta en el proyecto tiene un fichero de texto con el mismo nombre y extensión .meta

• Los ficheros .meta son necesarios. Mantenlos junto a su fichero

• Sólo son necesarias las carpetas Assets, Packages y ProjectSettings.

• No guardar ficheros fuera de la carpeta de Assets

• El resto de carpetas se reconstruyen a partir de estas y no deberían almacenarse en el sistema de control de versiones.

• A la hora de mover ficheros entre carpetas, hacerlo siempre desde Unity, para que se mueva también el fichero .meta

•Separar las escenas de prueba en una carpeta separada
	-Y dentro, incluso se pueden separar en carpetas por autor
	
• Mantener los ficheros propios separados de los de terceros
	 -Crear, por ejemplo, una carpeta ThirdParty para librerías o paquetes de assets externos