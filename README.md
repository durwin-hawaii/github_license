This projects searches github repositories looking for repositories that do not have a license.

There is an option to send a request to the repository owner to create a license.
To enable this option you need to input your authorization token to the token member of the GitHub class
(line 33 of GitHub.cs as of this initial revision)

This project is ready to run in a docker container. I have added some batch/shell files to build it and run it:
build.bat/build.sh  - will fetch any new code from github then build the container.
debug.bat/debug.sh  - will run the docker container in interactive shell mode listening on port 8083.
run.bat/run.sh      - will run the docker container in daemon (background) mode listening on port 8083.
stop.bat/stop.sh    - will stop the container and remove the image.
attach.bat/attach.sh- will attach to the container so that you can interact with it.

If running on linux you might have to make the .sh files executable.
