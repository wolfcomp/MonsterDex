echo "> Deleting old"
rm -rf repo/testing/live/${INTERNAL_NAME}
echo "> Making new"
if [[ ${MESSAGE} =~ .*"[TEST]".* ]]; then
    mkdir repo/testing/live/${INTERNAL_NAME}
    cd repo/testing/live/${INTERNAL_NAME}
else
    rm -rf repo/stable/${INTERNAL_NAME}
    mkdir repo/stable/${INTERNAL_NAME}
    cd repo/stable/${INTERNAL_NAME}
fi
echo "[plugin]" >>manifest.toml
echo "repository = \"${GITHUB_SERVER_URL}/${GITHUB_REPOSITORY}.git\"" >>manifest.toml
echo "owners = [ \"${GITHUB_REPOSITORY_OWNER}\" ]" >>manifest.toml
echo "project_path = \"${INTERNAL_NAME}\"" >>manifest.toml
echo "commit = \"${GITHUB_SHA}\"" >>manifest.toml
echo "changelog = ${CHANGELOG}" >>manifest.toml
echo "version = \"${VERSION}\"" >>manifest.toml
echo "> Done"
cat manifest.toml
mkdir images
cd images
echo "> Downloading images"
curl -L "https://raw.githubusercontent.com/wolfcomp/MonsterDex/master/DeepDungeonDex/icon.png" -o icon.png
