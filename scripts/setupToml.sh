echo "> Deleting old"
rm -rf repo/stable/$1
rm -rf repo/testing/live/$1
rm -rf repo/testing/net6/$1
echo "> Making new"
if [ $2 =~ .*"[TEST]".* ]; then
    mkdir repo/testing/net6/$1
    cd repo/testing/net6/$1
else
    mkdir repo/stable/$1
    cd repo/stable/$1
fi
echo "[plugin]" >>manifest.toml
echo "repository = \"$3.git\"" >>manifest.toml
echo "owners = [ \"$4\" ]" >>manifest.toml
echo "project_path = \"\"" >>manifest.toml
echo "commit = \"$5\"" >>manifest.toml
echo "changelog = \"$6\"" >>manifest.toml
echo "version = \"$7\"" >>manifest.toml
echo "> Done"
cat manifest.toml
