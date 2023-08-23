# Sales Reporting System

A simple Sales Reporting System. 

Implemented features:

- token-based authentication via JWT
- role-based authorization
- database creation and sample data seeding
- fully-isolated integration tests

## Prerequisites

- Visual Studio 2022
- Docker Desktop
## Running the app

- build the docker images by running
    ```
    .\scripts\build.ps1
    ```
    ![](./docs/20230824040908.png)
- bring up the containers by running
    ```
    .\scripts\up.ps1
    ```
    ![](./docs/20230824040928.png)
- to populate the database with sample data, run
    ```
    .\scripts\seed.ps1
    ```
    ![](./docs/20230824041031.png)

### Exploring APIs

- the Swagger API explorer will be available at http://localhost:20080/swagger/index.html
![](./docs/20230824041132.png)

- login via the `/Auth` endpoint
    - credentials for admin
        ```json
        {
          "userName": "admin",
          "password": "admin"
        }
        ```
    - credentials for agent
        ```json
        {
          "userName": "agent1",
          "password": "agent1"
        }
        ```
    - ![](./docs/20230824041331.png)
- copy down the value of the `token` in the authentication response > click on `Authorize` > paste in the token:
    - ![](./docs/20230824041425.png)
- explore the APIs available
![](./docs/20230824041526.png)

### Cleaning up

- to clean up all running docker containers, run
```
.\scripts\down.ps1
```
![](./docs/20230824041650.png)

## Running integration tests

- bring up test containers by running
```
.\scripts\up.ps1 -test
```
![](./docs/20230824042028.png)
- run tests inside Visual Studio as usual
![](./docs/20230824042315.png)

### Cleaning up
- to clean up all running docker containers, run
```
.\scripts\down.ps1 -test
```