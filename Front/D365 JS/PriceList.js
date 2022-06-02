async function retrieveCurrencyFromPriceList(executionContext) {
	let formContext = executionContext.getFormContext();
	let priceListRef = formContext.getAttribute("cread_fk_price_list").getValue();
	if (priceListRef != null) {
		let priceListId = priceListRef[0].id;
		let priceList = await Xrm.WebApi.retrieveRecord('cread_price_list', priceListId, "?$select=_transactioncurrencyid_value");

		if (priceList != null) {

			let currencyId = priceList["_transactioncurrencyid_value"];
			let currencyName = priceList["_transactioncurrencyid_value@OData.Community.Display.V1.FormattedValue"];

			formContext.getAttribute("transactioncurrencyid").setValue([{
				id: currencyId,
				name: currencyName,
				entityType: "transactioncurrency"
			}]);
		}
	}
}

function disableCurrency(executionContext) {
	let formContext = executionContext.getFormContext()
	formContext.getControl('transactioncurrencyid').setDisabled(true)
}

async function autofillNameFromProduct(executionContext) {
	let formContext = executionContext.getFormContext();
	let product = formContext.getAttribute("cread_fk_my_product").getValue();
	if (product != null) {
		formContext.getAttribute("cread_name").setValue(product[0].name);
	}
}
async function retrievePriceFromInventory(executionContext) {
	let formContext = executionContext.getFormContext();
	let inventoryRef = formContext.getAttribute("cread_fk_inventory").getValue();
	if (inventoryRef != null) {
		let inventoryId = inventoryRef[0].id;
		let priceList = await Xrm.WebApi.retrieveRecord('cread_inventory', inventoryId, "?$select=_cread_fk_price_list_value");
		priceList = await Xrm.WebApi.retrieveRecord('cread_price_list', priceList["_cread_fk_price_list_value"], "?$select=_transactioncurrencyid_value");
		if (priceList != null) {
			if (priceList != null) {
				let currencyId = priceList["_transactioncurrencyid_value"];
				let currencyName = priceList["_transactioncurrencyid_value@OData.Community.Display.V1.FormattedValue"];
				formContext.getAttribute("transactioncurrencyid").setValue([{
					id: currencyId,
					name: currencyName,
					entityType: "transactioncurrency"
				}]);
			}
		}
	}
}
async function retrievePricePerUnit(executionContext) {
	let formContext = executionContext.getFormContext();
	let inventoryRef = formContext.getAttribute("cread_fk_inventory").getValue();
	let productRef = formContext.getAttribute("cread_fk_my_product").getValue();

	if (inventoryRef != null && productRef != null) {
		let inventoryId = inventoryRef[0].id.toLowerCase().replace(/[{}]/g, "");
		let myProductId = productRef[0].id.toLowerCase().replace(/[{}]/g, "");
		let fetchXml = `<fetch version="1.0" output-format="xml-platform" mapping="logical" distinct="true">
                            <entity name="cread_inventory">
                            <attribute name="cread_inventoryid"/>
                            <attribute name="cread_fk_price_list"/>
                            <attribute name="cread_name"/>
                            <attribute name="createdon"/>
                            <order attribute="cread_name" descending="false"/>
                            <filter type="and">
                            <condition attribute="cread_inventoryid" operator="eq" value="${inventoryId}"/>
                            </filter>
                            <link-entity name="cread_price_list" from="cread_price_listid" to="cread_fk_price_list" link-type="inner" alias="aa">
                            <link-entity name="cread_price_list_items" from="cread_fk_price_list" to="cread_price_listid" link-type="inner" alias="ai">
                            <attribute name="cread_fk_my_product" />
                            <attribute name="cread_mon_price"/>
                            <filter type="and">
                            <condition attribute="cread_fk_my_product" operator="eq" value="${myProductId}"/>
                            </filter>
                            </link-entity>
                            </link-entity>
                            </entity>
                        </fetch>`
		fetchXml = "?fetchXml=" + encodeURIComponent(fetchXml);
		let PriceListItem = await Xrm.WebApi.retrieveMultipleRecords('cread_inventory', fetchXml);
		if (PriceListItem.entities.length == 0) {
			let productDefaultPrice = await Xrm.WebApi.retrieveRecord('cread_my_products', myProductId, "?$select=cread_price_per_unit");
			formContext.getAttribute('cread_price_per_unit').setValue(productDefaultPrice.cread_price_per_unit)
		} else {
			formContext.getAttribute('cread_price_per_unit').setValue(PriceListItem.entities[0]['ai.cread_mon_price'])
		}
	}
}