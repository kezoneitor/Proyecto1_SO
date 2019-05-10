CREATE TABLE palabra 
(
	categoria VARCHAR(100) NOT NULL,
	palabra VARCHAR (100) NOT NULL
	
);

CREATE TABLE url 

(

	id_url SERIAL PRIMARY KEY,
	texto VARCHAR(1000) NOT NULL,
	clasificacion VARCHAR (100) NOT NULL
);

CREATE TABLE categoria 
(
	id_url_cat INTEGER,
	nombre VARCHAR(100) NOT NULL,
	cantPalabras INTEGER,
	cantCoincidencias INTEGER,
	probabilidad DECIMAL,

	FOREIGN KEY (id_url_cat) REFERENCES url (id_url)
);

--hace la insercion de la url y se autoincrementa el id
INSERT INTO url (texto, clasificacion) VALUES ('www.thehobbit2.com', 'Tolkiene');
--hace la inserción con indicandole el id
INSERT INTO categoria (id_url_cat, nombre,cantPalabras,cantCoincidencias, probabilidad) VALUES (1048577, 'Mecagoenlaputisimamadreperros.com', 0,8,0.0006);

--hace la inserción para las palabras
INSERT INTO palabra (categoria, palabra) VALUES ('arts', 'casa');

---consultas
SELECT * FROM palabra
SELECT * FROM url order by id_url
SELECT * FROM categoria

--hace la consulta para sacar cada url con su categoria mediante la id
SELECT u.id_url, u.texto, c.nombre FROM url u INNER JOIN categoria c ON u.id_url = c.id_url_cat

--limit para mostar la cantidad de lineas 
--offset que es el inicio de X fila de la BD 
SELECT * FROM palabra limit 5 offset 10

--tamaño de la base de datos
SELECT COUNT (*) FROM palabra

--borrar todos los datos
DELETE FROM categoria


