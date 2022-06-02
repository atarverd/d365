let productLookupPointer = null;

async function filterContact(executionContext) {

	let Form = executionContext.getFormContext();
	let customerRef = Form.getAttribute('cread_fk_customer').getValue()
	let contactId = []

	if (productLookupPointer != null)
		Form.getControl("cread_fk_my_contacts").removePreSearch(productLookupPointer);

	if (customerRef != null) {
		let fetchXml = `<fetch version="1.0" output-format="xml-platform" mapping="logical" distinct="false">
        <entity name="cread_position">
        <attribute name="cread_positionid"/>
        <attribute name="cread_name"/>
        <attribute name="cread_fk_contact"/>
        <order attribute="cread_name" descending="false"/>
        <filter type="and">
        <condition attribute="cread_fk_account" operator="eq"  value="${customerRef[0].id}"/>
        </filter>
        </entity>
        </fetch>`
		fetchXml = "?fetchXml=" + encodeURIComponent(fetchXml);
		let contacts = await Xrm.WebApi.retrieveMultipleRecords('cread_position', fetchXml);
		for (let i = 0; i < contacts.entities.length; ++i)
			contactId[i] = contacts.entities[i]._cread_fk_contact_value;

		productLookupPointer = filterFunction.bind({
			"contactId": contactId
		});
		Form.getControl("cread_fk_my_contacts").addPreSearch(productLookupPointer);
	}
}

function filterFunction(executionContext) {
	let Form = executionContext.getFormContext();
	let contactFilter = `<filter type="or">`
	for (let i = 0; i < this.contactId.length; ++i) {
		contactFilter += `<condition attribute="cread_my_contactid" operator="eq" value="${this.contactId[i]}"/>`
	}
	contactFilter += `</filter>`;
	Form.getControl("cread_fk_my_contacts").addCustomFilter(contactFilter, "cread_my_contact");
}