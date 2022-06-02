let accountId = Xrm.Page.data.entity.getId();

let allPositions = [];
let step = 10;
let startIndex = 0;
let allAccounts = [];
let allContacts = [];
let allPositionOptions = [];

function OnScroll() {
	startIndex += step;
	for (let i = startIndex;
		(i < startIndex + step) && (i < allPositions.length); i++)
		addRow(allPositions[i]);
}
async function onLoad() {
	allAccounts = await getAllAccounts();
	allContacts = await getAllContacts();
	allPositionOptions = getAllPositions();
	allPositions = await getPositions();
	for (let i = startIndex;
		(i < startIndex + step) && (i < allPositions.length); i++) {
		await addRow(allPositions[i]);
	}
	document.getElementById("add").addEventListener(
		"click",
		function() {
			addRow();
		},
		false
	);
	document.addEventListener("scroll", OnScroll);
	document.getElementById("save").addEventListener("click", saveRecord);
	document.getElementById("delete").addEventListener("click", deleteRecord);
	document.getElementById("refresh").addEventListener("click", () => refreshWebResource("WebResource_position_grid_html"));
}

async function getPositions() {

	let rows = [];

	let accountId = Xrm.Page.data.entity.getId();

	let fetchXml = `<fetch version="1.0" output-format="xml-platform" mapping="logical" distinct="false">
    <entity name="cread_position">
    <attribute name="cread_positionid"/>
    <attribute name="cread_os_position"/>
    <attribute name="cread_employmentdate"/>
    <attribute name="cread_fk_contact"/>
    <attribute name="cread_fk_account"/>
    <order attribute="cread_fk_account" descending="false"/>
    <filter type="and">
        <condition attribute="cread_fk_account" operator="eq"  value="${accountId}"/>
    </filter>
    </entity>
    </fetch>`;

	fetchXml = "?fetchXml=" + encodeURIComponent(fetchXml);
	let positions = await Xrm.WebApi.retrieveMultipleRecords('cread_position', fetchXml);
	for (let i = 0; i < positions.entities.length; i++) {
		let line = positions.entities[i];
		if (line["_cread_fk_account_value"] != null) {
			accountId = accountId.toLowerCase().replace(/[{}]/g, "");
			let employmentDate = line["cread_employmentdate"];
			let contactId = line["_cread_fk_contact_value"]
			let positionRecordId = line["cread_positionid"]
			let positionId = line["cread_os_position"]

			let accountName = allAccounts.find(f => f.value == accountId.toLowerCase()).label;

			let contactName = allContacts.find(f => f.value == contactId).label;
			let positionName = allPositionOptions.find(f => f.value == positionId).label;
			rows.push({
				"accountName": accountName,
				"contactId": contactId,
				'positionId': positionId,
				"positionName": positionName,
				"positionRecordId": positionRecordId,
				"employmentDate": employmentDate,
				"accountId": accountId,
				'contactName': contactName
			});

		}
	}

	return rows;
}

function addOptions(a, optionDetails, optionDetailsInput, id) {
	let option
	if (a) {
		for (let i = 0; i < optionDetails.length; i++) {
			if (id == optionDetails[i].value)
				option = `<option selected value='${optionDetails[i].value}'>${optionDetails[i].label}</option>`
			else
				option = ` <option value='${optionDetails[i].value}'>${optionDetails[i].label}</option>`

			optionDetailsInput.insertAdjacentHTML("beforeend", option);

		}
	} else {
		for (let i = 0; i < optionDetails.length; i++) {
			option = `<option  value='${optionDetails[i].value}'>${optionDetails[i].label}</option>`
			optionDetailsInput.insertAdjacentHTML("beforeend", option);

		}
		option = `<option selected  value="blank"'>______</option>`
		optionDetailsInput.insertAdjacentHTML("beforeend", option);
	}
}

async function addRow(row = null) {

	var table = document.getElementById("tbody");
	var newRow = document.createElement('tr');
	var accountName = document.createElement('td');
	var contact = document.createElement('td');
	var date
	var nameId
	var contactId
	var positionId
	var employmentDate = document.createElement('td');
	var positionName = document.createElement('td');
	var checkbox = document.createElement('input');


	var accountNameInput = document.createElement('select');
	var contactInput = document.createElement('select');
	var employmentDateInput = document.createElement('input');
	var positionNameInput = document.createElement('select');
	let a
	accountName.setAttribute("id", "Account");
	contact.setAttribute("id", "Contact");
	positionName.setAttribute("id", "Position");
	employmentDate.setAttribute("id", "Date");
	if (row) {
		newRow.setAttribute("id", row.positionRecordId);
		date = row.employmentDate.slice(0, 10);
		nameId = row.accountId.toLowerCase().replace(/[{}]/g, "");
		contactId = row.contactId;
		positionId = row.positionId;
		checkbox.setAttribute("type", "checkbox");
		checkbox.setAttribute("name", "mycheckboxes");
		checkbox.setAttribute("id", row.positionRecordId);

		a = 1
	} else {
		newRow.setAttribute("id", 'blank');
		a = 0
	}


	employmentDateInput.setAttribute("value", date);
	employmentDateInput.setAttribute("type", "date");
	employmentDate.appendChild(employmentDateInput);



	accountName.appendChild(accountNameInput);
	addOptions(a, allAccounts, accountNameInput, nameId)

	contact.appendChild(contactInput);
	addOptions(a, allContacts, contactInput, contactId)

	positionName.appendChild(positionNameInput);
	addOptions(a, allPositionOptions, positionNameInput, positionId);
	newRow.appendChild(accountName);
	newRow.appendChild(contact);
	newRow.appendChild(employmentDate);
	newRow.appendChild(positionName);
	if (a) newRow.appendChild(checkbox);
	if (newRow.id == 'blank') table.insertBefore(newRow, table.firstChild)
	else table.appendChild(newRow);
}
async function deleteRecord() {
	var checkedBoxes = document.querySelectorAll('input[name=mycheckboxes]:checked');

	for (let i = 0; i < checkedBoxes.length; ++i) {
		let result = await Xrm.WebApi.deleteRecord("cread_position", checkedBoxes[i].id);
	}

	Xrm.Page.getControl("Positions").refresh();
	refreshWebResource("WebResource_position_grid_html");
}
async function saveRecord() {
	let table = document.getElementById("dataTable");
	let data = {}
	for (var i = 1, row; row = table.rows[i]; i++) {
		var nameId;
		var contactId;
		var positionId;
		var date;
		for (var j = 0, col; col = row.cells[j]; j++) {
			var e = col.getElementsByTagName("select")[0];
			if (col.id == "Account") {
				nameId = e.options[e.selectedIndex].value
			}
			if (col.id == "Contact") {
				contactId = e.options[e.selectedIndex].value
			}
			if (col.id == "Date") {
				e = col.getElementsByTagName("input")[0];
				date = e.value
			}
			if (col.id == "Position") {
				positionId = e.options[e.selectedIndex].value
			}
		}
		if (nameId == 'blank' || contactId == 'blank' || positionId == 'blank' || date == '') {
			alert("Please fill all sections")
			return 0
		}
		data["cread_fk_account@odata.bind"] = `/cread_my_accounts(${nameId})`;
		data["cread_fk_contact@odata.bind"] = `/cread_my_contacts(${contactId})`;
		data["cread_os_position"] = positionId;
		data["cread_employmentdate"] = date;
		if (row.id == "blank")
			result = await Xrm.WebApi.createRecord("cread_position", data);
		else
			result = await Xrm.WebApi.updateRecord("cread_position", row.id, data);
	}
	Xrm.Page.getControl("Positions").refresh();
	refreshWebResource("WebResource_data_grid");
}

function refreshWebResource(webResourceName) {
	let gridControl = parent.Xrm.Page.getControl(webResourceName);
	if (gridControl && gridControl.getObject()) {
		const src = gridControl.getObject().src;
		gridControl.getObject().src = '';
		gridControl.getObject().src = src;
	}
}
async function getAllAccounts() {
	let fetchXml = `<fetch version="1.0" output-format="xml-platform" mapping="logical" distinct="false">
    <entity name="cread_my_account">
      <attribute name="cread_my_accountid" />
      <attribute name="cread_name" />
      <attribute name="createdon" />
      <order attribute="cread_name" descending="false" />
    </entity>
  </fetch>`
	fetchXml = "?fetchXml=" + encodeURIComponent(fetchXml);
	let accounts = await Xrm.WebApi.retrieveMultipleRecords('cread_my_account', fetchXml);
	let data = []
	for (let i = 0; i < accounts.entities.length; i++) {

		data.push({
			'value': accounts.entities[i].cread_my_accountid,
			'label': accounts.entities[i].cread_name
		})
	}
	return data
}

async function getAllContacts() {
	let fetchXml = `<fetch version="1.0" output-format="xml-platform" mapping="logical" distinct="false">
    <entity name="cread_my_contact">
    <attribute name="cread_my_contactid"/>
    <attribute name="cread_name"/>
    <attribute name="createdon"/>
    <order attribute="cread_name" descending="false"/>
    </entity>
    </fetch>`
	fetchXml = "?fetchXml=" + encodeURIComponent(fetchXml);
	let contacts = await Xrm.WebApi.retrieveMultipleRecords('cread_my_contact', fetchXml);
	let data = []
	for (let i = 0; i < contacts.entities.length; i++) {

		data.push({
			'value': contacts.entities[i].cread_my_contactid,
			'label': contacts.entities[i].cread_name
		})
	}
	return data
}

function getAllPositions() {
	return [{
			'value': 826080000,
			'label': 'CTO'
		},
		{
			'value': 826080001,
			'label': 'CEO'
		}

	]
}

function autoFillName(executionContext) {
	let Form = executionContext.getFormContext();
	let fullName = Form.getAttribute('cread_slot_first_name').getValue() + " " + Form.getAttribute('cread_slot_last_name').getValue();
	Form.getAttribute('cread_name').setValue(fullName);
}

function sort(e) {


	let sortColumnName = "";
	let sortType = "ascending";

	let ascendingSymbol = "&#x25b4;";
	let descendingSymbol = "&#x25be;";
	let ascendingSymbolSearch = '\u25b4';
	let descendingSymbolSearch = '\u25be';


	let columnName = e.innerHTML;

	if (columnName.indexOf(ascendingSymbolSearch) > -1) {
		columnName = descendingSymbol + columnName.substring(1, columnName.length);
		sortType = "descending";
	} else
	if (columnName.indexOf(descendingSymbolSearch) > -1) {
		columnName = ascendingSymbol + columnName.substring(1, columnName.length);
		sortType = "ascending";
	} else {
		columnName = ascendingSymbol + columnName;
		sortType = "ascending";
	}

	sortColumnName = e.getAttribute("fieldName");

	e.innerHTML = columnName;

	let rows = document.getElementById("dataTable").rows;
	let row = rows[0];
	let cells = row.cells;
	for (let j = 0; j < cells.length; j++) {
		if (e !== cells[j]) {
			let otherColumnName = cells[j].innerHTML;
			if (otherColumnName.indexOf(ascendingSymbolSearch) > -1) {
				cells[j].innerHTML = otherColumnName.substring(1, columnName.length);
			} else
			if (otherColumnName.indexOf(descendingSymbolSearch) > -1) {
				cells[j].innerHTML = otherColumnName.substring(1, columnName.length);
			}
		}
	}

	sortRows(sortColumnName, sortType);
}

function sortRows(sortColumnName, sortType) {
	startIndex = 0
	removeAllRows();
	let sortedPositions = []


	if (sortType == "ascending") {
		if (sortColumnName == "employmentDate") {
			sortedPositions = allPositions.sort(function(a, b) {
				return a.employmentDate.split('/').reverse().join('') == b.employmentDate.split('/').reverse().join('') ? 0 : a.employmentDate.split('/').reverse().join('') > b.employmentDate.split('/').reverse().join('') ? 1 : -1;
			});
		} else
			sortedPositions = allPositions.sort((a, b) => {
				return a[sortColumnName].toLowerCase() == b[sortColumnName].toLowerCase() ? 0 : a[sortColumnName].toLowerCase() > b[sortColumnName].toLowerCase() ? 1 : -1;
			})
	} else {
		if (sortColumnName == "employmentDate") {
			sortedPositions = allPositions.sort(function(a, b) {
				return a.employmentDate.split('/').reverse().join('') == b.employmentDate.split('/').reverse().join('') ? 0 : a.employmentDate.split('/').reverse().join('') > b.employmentDate.split('/').reverse().join('') ? -1 : 1;
			});

		} else
			sortedPositions = allPositions.sort((a, b) => {
				return a[sortColumnName].toLowerCase() == b[sortColumnName].toLowerCase() ? 0 : a[sortColumnName].toLowerCase() > b[sortColumnName].toLowerCase() ? -1 : 1;
			})
	}

	for (let i = startIndex;
		(i < startIndex + step) && (i < sortedPositions.length); i++)
		addRow(sortedPositions[i]);

}

function removeAllRows() {

	document.getElementById("dataTable").getElementsByTagName("tbody")[0].innerHTML = ''
}

function saveAll() {
	Xrm.Page.getControl("WebResource_data_grid").getObject().contentWindow.window[saveRecord()];
}