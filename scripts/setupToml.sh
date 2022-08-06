echo "> Deleting old"
rm -rf repo/stable/${ env.INTERNAL_NAME }
rm -rf repo/testing/live/${ env.INTERNAL_NAME }
rm -rf repo/testing/net6/${ env.INTERNAL_NAME }
echo "> Making new"
if [ ${ github.event.head_commit.message } == *[TEST]*]; then
    mkdir repo/testing/net6/${ env.INTERNAL_NAME }
    cd repo/testing/net6/${ env.INTERNAL_NAME }
else
    mkdir repo/stable/${ env.INTERNAL_NAME }
    cd repo/stable/${ env.INTERNAL_NAME }
fi
echo "[plugin]" >> manifest.toml
echo "repository = \"${ env.GITHUB_SERVER_URL }/${ env.GITHUB_REPOSITORY }.git\"" >> manifest.toml
echo "owners = [ \"${ env.GITHUB_REPOSITORY_OWNER }\" ]" >> manifest.toml
echo "project_path = \"\"" >> manifest.toml
echo "commit = \"${ env.GITHUB_SHA }\"" >> manifest.toml
echo "changelog = \"${ steps.version.clean.replaced }\"" >> manifest.toml
echo "version = \"${ steps.version.outputs.new_version }\"" >> manifest.toml
echo "> Done"
cat manifest.toml