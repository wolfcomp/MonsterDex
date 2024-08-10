cd repo
echo "> Adding and committing"
git add --all
git commit --all -m "Updating ${PUBLIC_NAME}"
echo "> Pushing to origin"
git push --force --set-upstream origin "${PUBLIC_NAME}"
prRepo="goatcorp/DalamudPluginsD17"
prNumber=$(gh api repos/${prRepo}/pulls | jq ".[] | select(.head.ref == \"${PUBLIC_NAME}\") | .number")
if [[ ${MESSAGE} =~ .*"[TEST]".* ]]; then
    prTitle="[Testing] ${PUBLIC_NAME} ${VERSION}"
else
    prTitle="${PUBLIC_NAME} ${VERSION}"
fi
prBody="${CHANGELOG}"
if [ "${prNumber}" ]; then
    echo "> Editing existing PR"
    gh api "repos/${prRepo}/pulls/${prNumber}" --silent --method PATCH -f "title=${prTitle}" -f "body=${prBody}" -f "state=open"
else
    echo "> Creating PR"
    gh pr create --repo "${prRepo}" --head "${GITHUB_REPOSITORY_OWNER}:${PUBLIC_NAME}" --base "main" --title "${prTitle}" --body "${prBody}"
fi
