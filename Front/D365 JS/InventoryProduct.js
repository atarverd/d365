async function productExistsNotification(executionContext) {
	let formContext = executionContext.getFormContext();
	formContext.getControl("cread_fk_my_product").clearNotification(1);
	let inventory = formContext.getAttribute("cread_fk_inventory").getValue();
	let product = formContext.getAttribute("cread_fk_my_product").getValue();
	let inventoryId = inventory[0].id.toLowerCase().replace(/[{}]/g, "");

	let fetchXml = `<fetch version="1.0" output-format="xml-platform" mapping="logical" distinct="false">
    <entity name="cread_inventory_product">
      <attribute name="cread_inventory_productid" />
      <attribute name="cread_name" />
      <attribute name="createdon" />
      <order attribute="cread_name" descending="false" />
      <filter type="and">
      <condition attribute="cread_fk_inventory" operator="eq"  value="${inventoryId}" />
    </filter>
    </entity>
  </fetch>`
	fetchXml = "?fetchXml=" + encodeURIComponent(fetchXml);
	let inventoryProducts = await Xrm.WebApi.retrieveMultipleRecords('cread_inventory_product', fetchXml);

	if (product != null)
		for (let i = 0; i < inventoryProducts.entities.length; ++i)
			if (product[0].name == inventoryProducts.entities[i].cread_name)
				result = formContext.getControl("cread_fk_my_product").setNotification("This product already exists", 1);
}