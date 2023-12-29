# CRUD Project for Crypto OrderBook Data

## Introduction
The project consists of a WebApplication that allow users to subscribe and unsubscribe the first row of crypto orderbooks (explained below)
in different exchanges, for different frequencies of data collection. All possible crypto pairs, exchanges and frequencies can be obtained
with a GET request to teh endpoint  **/api/ValidRequestParams**.
Both the active subscriptions defined by the user, and the data values stores when collecting, are saved in a MySQL DB, which can be configured
as will be explained further.
The project also implements SwaggerUI to facilitate testing when running locally.
subscriber that accesses a crypto exchange API (ex: Binance), retrieves the first row of the orderbook using an HTTP method,
and sends that data to a callback class, that stores the data in an MySQL DB.

Definition of orderbook: list of buy and sell orders of a specific financial instrument, organized by price level.
Example of an orderbook:
|    #Order   | Price | Amount | Total |
| :---------: | :---: | :----: | :---: |
| #SellOrder3 | 19.95 |  420   |  870  |
| #SellOrder2 | 19.89 |  300   |  300  |
| #SellOrder1 | 19.87 |  150   |  150  |
| ----------- | ----- |  ----  |  ---- |
| #BuyOrder1  | 19.50 |  100   |  100  |
| #BuyOrder2  | 19.48 |  220   |  320  |
| #BuyOrder3  | 19.47 |  500   |  820  |

A real orderbook can be seen at: https://www.binance.com/en/trade/BTC_USDT?type=spot in the left corner of the screen.


## Setup

### LocalTesting
After cloning the code from github, by default the application database used will be maintained in memory,
but the user can also set a working MySQL database in the "AppDbConnectionString" variable in "appsettings.json".
Then, to automatically create the database schemas and tables needed, the user can add a migration via the command "dotnet ef migrations add MyMigration"
and run "dotnet ef database update" in the cmd to update the database, inside the project file folder.
In a windows enviroment, the user can use simpler commands in the Package Manager Console, just Add-Migration "{MigrationName}" and Update-Database.
Just running the program with the option "http" in Visual Studio, should open the web browser in the designed localhost port by swagger,
make sure the URL looks something like this: "http://localhost:5265/swagger", Swagger can add an extra "/", or not add the "/swagger" sometimes.
After launching, the available API Requests such be visible. Worth checking the port set in the "lauchSettings.json", in case the projects automatically
chooses another port.


### Interacting With the published API
In case of testing with the public published api, all requests must be made for **HOST** = jymzdvmivm.ap-southeast-1.awsapprunner.com, and **HTTPS protocol**
such as **https://jymzdvmivm.ap-southeast-1.awsapprunner.com/swagger/index.html**. All requests can be made directly, or from the SwaggerUI in browser.



## Usage
After the system is running, the user can make a GET request to the endpoint **/api/ValidRequestParams** 
in order to retrieve the valid parameters for both subscriptions and retrieval of bookData are implemented in the code, and available for data collecting.

### Get ValidRequestParams
In order to retrieve valid parameters used when doing HTTP requests to the API, the user can make a GET request to the endpoint **/api/ValidRequestParams**

### Get Active Subscriptions
In order to remove an existing subscription, the user can make a **GET** request, to the endpoint **/api/Subscriptions**,
no additional parameters are needed.


### Add Subscription
In order to add a new subscription, the user can make a **PUT** request with the parameters:
id, sourceName, cryptoPairName and frequency, to the endpoint **/api/Subscriptions** such as the following example:
```json
{
  "id": 0,
  "sourceName": "Binance",
  "cryptoPair": "BTCUSDT",
  "frequencyMS": 30000
}
```

### Update Subscription
In order to update an existing subscription, the user can make a **POST** request with the parameters:
id, sourceName, cryptoPairName and frequency, to the endpoint **/api/Subscriptions** such as the following example:
```json
{
  "id": 0,
  "sourceName": "Binance",
  "cryptoPair": "BTCUSDT",
  "frequencyMS": 30000
}
```

### Remove Subscription
In order to remove an existing subscription, the user can make a **DELETE** request with the parameters:
id, to the endpoint **/api/Subscriptions** such as the following example:
```json
{
  "id": 0
}
```

### Retrieve BookData
In order to retrieve database bookvalues saved for a CryptoPair, the user can make a **POST** request with the parameters:
List of sourceNames, list of cryptoPairName and limitrows, to the endpoint **/api/RetrieveBookData** such as the following example:
```json
{
  "sourceNames": [
    "Binance",
    "Coinbase"
  ],
  "cryptoPairs": [
    "BTCUSDT",
    "ETHUSDT",
    "XRPUSDT",
    "SOLUSDT",
    "ADAUSDT"
  ],
  "limitRows": 10
}
```