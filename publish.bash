# ====== Редактируемые параметры ====== 

# ===================================== 


SCRIPT_DIR="$(cd -- "$(dirname -- "$0")" && pwd)"
cd "$SCRIPT_DIR" || exit 1

PORT=53499

docker run --rm -d --name plantuml_for_rdfs_doc -p $PORT:8080 plantuml/plantuml-server

until curl -sf http://localhost:$PORT > /dev/null; do
  echo "Waiting for plantuml..."
  sleep 2
done

cp $RDFSDOC_PATH_TO_RDFS ./out/rdfs.xml

docker pull gitea.simpleperson.ru/admin/rdfs-beauty-doc:latest

docker run --rm \
  -v ./out:/out \
  --net=host \
  -e RDFSDOC_PLANTUML_URL=http://localhost:$PORT \
  -e RDFSDOC_OUTPUT_PATH=/out \
  -e RDFSDOC_PATH_TO_RDFS=/out/rdfs.xml \
  -e RDFSDOC_TITLE \
  -e RDFSDOC_DESCRIPTION \
  -e RDFSDOC_COMMON_NAMESPACE \
  -e RDFSDOC_USE_NAMESPACE_FOR_PROPERTIES \
  gitea.simpleperson.ru/admin/rdfs-beauty-doc:latest

docker stop plantuml_for_rdfs_doc
docker build -t nginx_static_site .
docker tag nginx_static_site gitea.simpleperson.ru/admin/cim-ontology:latest
docker push gitea.simpleperson.ru/admin/cim-ontology:latest