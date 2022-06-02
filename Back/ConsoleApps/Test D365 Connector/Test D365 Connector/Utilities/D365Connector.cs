using Microsoft.Xrm.Tooling.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Test_D365_Connector.Model;

namespace Test_D365_Connector.Utilities {
  class D365Connector {

    private string D365username;
    private string D365password;
    private string D365URL;

    private CrmServiceClient service;

    public D365Connector(string D365username, string D365password, string D365URL) {
      this.D365username = D365username;
      this.D365password = D365password;
      this.D365URL = D365URL;

      string authType = "OAuth";
      string appId = "51f81489-12ee-4a9e-aaae-a2591f45987d";
      string reDirectURI = "app://58145B91-0C36-4500-8554-080854F2AC97";
      string loginPrompt = "Auto";

      string ConnectionString = string.Format("AuthType = {0};Username = {1};Password = {2}; Url = {3}; AppId={4}; RedirectUri={5};LoginPrompt={6}",
        authType, D365username, D365password, D365URL, appId, reDirectURI, loginPrompt);

      this.service = new CrmServiceClient(ConnectionString);
    }

    public void createInventoryProduct(Inventory inventory,decimal pricePerUnit, Guid currencyId,Product product,int userQuantity
        ) {
            decimal price=product.defaultPricePerUnit;
            if(pricePerUnit!=-1)
                price=pricePerUnit;
           
                 Entity inventoryProduct = new Entity("cread_inventory_product");
      inventoryProduct["cread_name"]=product.productName;
      inventoryProduct["cread_fk_inventory"]= new EntityReference("cread_inventory",inventory.inventoryId);
      inventoryProduct["cread_fk_my_product"] = new EntityReference("cread_my_products", product.productId);
      inventoryProduct["cread_price_per_unit"] = new Money(price);
      inventoryProduct["cread_quantity"] = userQuantity;

      service.Create(inventoryProduct);
        Console.WriteLine("Created!");
    }

    public Inventory getInventoryByName(string inventoryName) {
      Inventory inventoryObj = null;
      QueryExpression inventoryQuery = new QueryExpression {
        EntityName = "cread_inventory",
          ColumnSet = new ColumnSet("cread_name", "cread_fk_price_list"),
          Criteria = {
            Conditions = {
              new ConditionExpression("cread_name", ConditionOperator.Equal, inventoryName)
            }
          }
      };

      EntityCollection inventories = service.RetrieveMultiple(inventoryQuery);
      if (inventories.Entities.Count > 0) {
        Entity inventory = inventories.Entities[0];
        EntityReference priceList = inventory.GetAttributeValue < EntityReference > ("cread_fk_price_list");
        inventoryObj = new Inventory {
          inventoryId = inventory.Id,
            invName = inventory.GetAttributeValue < string > ("cread_name"),
            priceListId = priceList.Id,
            priceListName = priceList.Name
        };
      }
      return inventoryObj;
    }

    public Product getProductByName(string productName) {
      Product productObj = null;
      QueryExpression productQuery = new QueryExpression {
        EntityName = "cread_my_products",
          ColumnSet = new ColumnSet("cread_name", "cread_price_per_unit"),
          Criteria = {
            Conditions = {
              new ConditionExpression("cread_name", ConditionOperator.Equal, productName)
            }
          }
      };

      EntityCollection products = service.RetrieveMultiple(productQuery);

      if (products.Entities.Count > 0) {
        Entity product = products.Entities[0];
        productObj = new Product {
          productId = product.Id,
            productName = product.GetAttributeValue < string > ("cread_name"),
            defaultPricePerUnit = (Decimal) product.GetAttributeValue < Money > ("cread_price_per_unit").Value

        };
      }
      return productObj;
    }

    public InventoryProduct getInventoryProduct(Product product, Inventory inventory,string typeOfOperation,int userQuantity) {
            
      InventoryProduct inventoryProductObj = null;

      QueryExpression inventoryProductQuery = new QueryExpression {
        EntityName = "cread_inventory_product",
          ColumnSet = new ColumnSet("cread_name", "cread_fk_my_product", "cread_quantity", "cread_fk_inventory"),
          Criteria = {
            Conditions = {
              new ConditionExpression("cread_fk_my_product", ConditionOperator.Equal, product.productId),
              new ConditionExpression("cread_fk_inventory", ConditionOperator.Equal, inventory.inventoryId)

            }
          }
      };
      Entity PriceListRef = service.Retrieve("cread_price_list", inventory.priceListId, new ColumnSet("transactioncurrencyid"));
      EntityReference currency = PriceListRef.GetAttributeValue < EntityReference > ("transactioncurrencyid");
            
      QueryExpression PricePerUnitQuery = new QueryExpression {
        EntityName = "cread_price_list_items",
          ColumnSet = new ColumnSet("cread_fk_my_product", "cread_mon_price", "transactioncurrencyid"),
          Criteria = {
            Conditions = {
              new ConditionExpression("cread_fk_my_product", ConditionOperator.Equal, product.productId),
              new ConditionExpression("transactioncurrencyid", ConditionOperator.Equal, currency.Id),

            }
          }
      };
            
      EntityCollection inventoryProducts = service.RetrieveMultiple(inventoryProductQuery); 
       decimal pricePerUnit=-1;
      EntityCollection pricePerUnits = service.RetrieveMultiple(PricePerUnitQuery);
            if (pricePerUnits.Entities.Count > 0) {        
                 Entity price = pricePerUnits.Entities[0];

                 pricePerUnit =(Decimal)price.GetAttributeValue < Money > ("cread_mon_price").Value;

            }

      if (inventoryProducts.Entities.Count > 0) {
        Entity inventoryProduct = inventoryProducts.Entities[0];

        inventoryProductObj = new InventoryProduct {
          inventoryProductName = inventoryProduct.GetAttributeValue < string > ("cread_name"),
            inventoryProductQuantity = inventoryProduct.GetAttributeValue < int > ("cread_quantity"),
            inventoryProductId = inventoryProduct.Id,
        };
        if (typeOfOperation == "1" && userQuantity<=inventoryProductObj.inventoryProductQuantity)
           updateQuantity(inventoryProductObj.inventoryProductId, inventoryProductObj.inventoryProductQuantity-userQuantity);
        
        else if(typeOfOperation=="2")
           updateQuantity(inventoryProductObj.inventoryProductId, inventoryProductObj.inventoryProductQuantity+userQuantity);

        else 
            Console.WriteLine("error");
      }
          
       else {
        if(typeOfOperation == "2")
        createInventoryProduct(inventory,pricePerUnit, currency.Id,product,userQuantity);
        else 
            Console.WriteLine("error");
        
      }
      Console.ReadLine();
      return inventoryProductObj;
    }

    public void updateQuantity(Guid inventoryProductId, int quantity) {
      Entity productInventory = new Entity("cread_inventory_product");
      productInventory.Id = inventoryProductId;
      productInventory["cread_quantity"] = quantity;
      this.service.Update(productInventory);
            Console.WriteLine("Updated!");
    }
  }
}