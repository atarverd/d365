let productLookupPointer = null;

async function filterProduct(executionContext) {

	let Form = executionContext.getFormContext();
	let inventoryRef = Form.getAttribute("cread_fk_inventory").getValue();
	let productId = []

	if (productLookupPointer != null)
		Form.getControl("cread_fk_my_product").removePreSearch(productLookupPointer);

	if (inventoryRef != null) {
		let inventoryId = inventoryRef[0].id.toLowerCase().replace(/[{}]/g, "");
		let fetchXml = `<fetch version="1.0" output-format="xml-platform" mapping="logical" distinct="false">
        <entity name="cread_inventory_product">
        <attribute name="cread_inventory_productid"/>
        <attribute name="cread_name"/>
        <attribute name="cread_fk_my_product"/>
        <order attribute="cread_name" descending="false"/>
        <filter type="and">
        <condition attribute="cread_fk_inventory" operator="eq"  value="${inventoryId}"/>
        </filter>
        </entity>
        </fetch>`

		fetchXml = "?fetchXml=" + encodeURIComponent(fetchXml);
		let products = await Xrm.WebApi.retrieveMultipleRecords('cread_inventory_product', fetchXml);
		for (let i = 0; i < products.entities.length; ++i)
			productId[i] = products.entities[i]._cread_fk_my_product_value;

		productLookupPointer = filterFunction.bind({
			"productId": productId
		});
		Form.getControl("cread_fk_my_product").addPreSearch(productLookupPointer);
	}
}

function filterFunction(executionContext) {
	let Form = executionContext.getFormContext();
	let productsFilter = `<filter type="or">`
	for (let i = 0; i < this.productId.length; ++i) {
		productsFilter += `<condition attribute="cread_my_productsid" operator="eq" value="${this.productId[i]}"/>`
	}
	productsFilter += `</filter>`;
	Form.getControl("cread_fk_my_product").addCustomFilter(productsFilter, "cread_my_products");
}