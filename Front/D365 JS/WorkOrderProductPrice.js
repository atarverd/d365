async function workOrderProductPrice(executionContext){
    
   let formContext = executionContext.getFormContext();
	let inventoryRef = formContext.getAttribute("cread_fk_inventory").getValue();
	let productRef = formContext.getAttribute("cread_fk_my_product").getValue();
  let quantity = formContext.getAttribute("cread_quantity").getValue();

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
			formContext.getAttribute('cread_fk_price_per_unit').setValue(productDefaultPrice.cread_price_per_unit)
		} else {
			formContext.getAttribute('cread_fk_price_per_unit').setValue(PriceListItem.entities[0]['ai.cread_mon_price'])
		}
	}
  let total=formContext.getAttribute("cread_fk_price_per_unit").getValue()*quantity
  console.log(total)
  formContext.getAttribute("cread_mon_total_amount").setValue(total)
}