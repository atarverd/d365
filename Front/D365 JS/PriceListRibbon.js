async function initializePriceList(Form) {
    let priceListId = Form.data.entity.getId().toLowerCase().replace(/[{}]/g, '')
    let currency = Form.getAttribute('transactioncurrencyid').getValue()
    let currencyId=currency[0].id.toLowerCase().replace(/[{}]/g,'')

    let fetchXml = `<fetch version="1.0" output-format="xml-platform" mapping="logical" distinct="false">
                      <entity name="cread_price_list_items">
                        <attribute name="cread_price_list_itemsid" />
                        <attribute name="cread_name" />
                        <attribute name="createdon" />
                        <order attribute="cread_name" descending="false" />
                        <filter type="and">
                          <condition attribute="cread_fk_price_list" operator="eq" uiname="AMD" uitype="cread_price_list" value="{8F75B0FE-18A5-EC11-B3FE-0022489B0281}" />
                        </filter>
                      </entity>
                    </fetch>`
    fetchXml = "?fetchXml=" + encodeURIComponent(fetchXml);
    let items = await Xrm.WebApi.retrieveMultipleRecords('cread_price_list_items', fetchXml);
    let id = []
    if (items != null) {
        console.log(1)
        for (let i = 0; i < items.entities.length; i++) {
            id[i] = items.entities[i].cread_price_list_itemsid
            let result = await Xrm.WebApi.deleteRecord("cread_price_list_items", id[i]);
        }
    }
    fetchXml = `<fetch version="1.0" output-format="xml-platform" mapping="logical" distinct="false">
                  <entity name="cread_my_products">
                    <attribute name="cread_my_productsid" />
                    <attribute name="cread_name" />
                    <attribute name="createdon" />
                    <order attribute="cread_name" descending="false" />
                  </entity>
                </fetch>`
    fetchXml = "?fetchXml=" + encodeURIComponent(fetchXml);
    let products = await Xrm.WebApi.retrieveMultipleRecords('cread_my_products', fetchXml);
    console.log(products)
    let data = {};
    for (let i = 0; i < products.entities.length; i++) {
        data["cread_fk_price_list@odata.bind"] = `/cread_price_lists(${priceListId})`;
        console.log(products.entities[i].cread_my_productsid)
        data["cread_fk_my_product@odata.bind"] = `/cread_my_productses(${products.entities[i].cread_my_productsid})`;
        data["transactioncurrencyid@odata.bind"] = `/transactioncurrencies(${currencyId})`;
        data["cread_mon_price"] = 1;
        console.log(data)
        result = await Xrm.WebApi.createRecord("cread_price_list_items", data);
    }
    Xrm.Page.getControl("price_list_item").refresh();
}