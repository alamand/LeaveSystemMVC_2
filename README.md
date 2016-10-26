# SourceCodeRepo
This repository will only contain the source code for the implementation of the solution for the ICT333 project.

# Tutorials and guides
These tutorials can be helpful if you are just starting out with git

https://www.youtube.com/watch?v=Y9XZQO1n_7c

https://git-scm.com/book/en/v2/Git-Branching-Basic-Branching-and-Merging

If there is anything else that you would like a guide on or simply have more questions, please contact me (Hamza).

# Follow these steps in order to set up this repository locally for the first time

NOTE: Please follow these steps starting from the super repository in the organization. Also make sure that you already have git installed on your system.

1. On the top right of this repository page, click the fork button and fork this
repository to your account.

2. Go to the fork of this repository that you made on your account.

3. Click on the "Clone or download" in the root page of the forked repository and copy the link provided in the dropdown.

4. Now make a folder on your personal computer where you want to set up this repository locally.

5. Enter that folder, right click in the directory and click "Git bash here".

6. In the git bash enter the following commands

	git init
	
	git remote add origin (the link you copied here without brackets)
	
	git pull origin
	
These steps should set up the git repository for you locally.

Please make sure that you also go through the following document at least once as it is a great resource on a forking workflow:

https://gist.github.com/Chaser324/ce0505fbed06b947d962

# Staging and commiting changes to your repository
git add (file name with extension)

OR

git add . (putting the period will stage all changes to the repository)

git commit -m '(commit message here)'

# Creating a new branch for your repository
git branch (branch name)

# Switching your active branch
git checkout (branch name)