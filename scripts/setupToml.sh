echo "> Deleting old"
rm -rf repo/stable/${INTERNAL_NAME}
rm -rf repo/testing/live/${INTERNAL_NAME}
rm -rf repo/testing/net6/${INTERNAL_NAME}
echo "> Making new"
if [ ${MESSAGE} =~ .*"[TEST]".* ]; then
    mkdir repo/testing/net6/${INTERNAL_NAME}
    cd repo/testing/net6/${INTERNAL_NAME}
else
    mkdir repo/stable/${INTERNAL_NAME}
    cd repo/stable/${INTERNAL_NAME}
fi
echo "[plugin]" >>manifest.toml
echo "repository = \"${URL}.git\"" >>manifest.toml
echo "owners = [ \"${GITHUB_REPOSITORY_OWNER}\" ]" >>manifest.toml
echo "project_path = \"\"" >>manifest.toml
echo "commit = \"${GITHUB_SHA}\"" >>manifest.toml
echo "changelog = \"${CHANGELOG}\"" >>manifest.toml
echo "version = \"${VERSION}\"" >>manifest.toml
echo "> Done"
cat manifest.toml
