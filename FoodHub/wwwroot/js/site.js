// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.


"use strict";

var cardId = 0;
var orderId = 0;
function orderAccepted(id,ordId) {
    var order = document.getElementById("order_" + id);
    var email = document.getElementById("email_" + id).children[0].innerHTML;
    order.innerHTML = "Order In Progress";
    document.getElementById("accept_" + id).style.display = "none";
    document.getElementById("decline_" + id).style.display = "none";

    var btn = document.createElement("BUTTON");
    btn.setAttribute("class", "btn btn-info");
    btn.setAttribute("onClick", "finishOrder("+id+","+ordId+")");
    btn.innerHTML = "Finish Order";
    var card = document.getElementById("card_"+id);
    card.appendChild(btn);

    //update state
    doPost({ "state": "In Progress", "orderId": ordId, "email": email});
 
}

function doPost(data) {
    const options = {
        method: 'POST',
        headers: {
            "Content-Type": "application/json",
        },
        body: JSON.stringify(data)
    };
    fetch("http://localhost:54058/orders/UpdateOrder", options);
}

function orderDenied(id, ordId) {
    var order = document.getElementById("order_" + id);
    var modal = document.getElementById("myModal");
    modal.style.display = "block";
    cardId = id;
    orderId = ordId;
}

function cancelClicked() {
    var modal = document.getElementById("myModal");
    modal.style.display = "none";
}

function denyOrder() {
    var item = document.getElementById("item_" + cardId);
    var email = document.getElementById("email_" + cardId).children[0].innerHTML;
    item.style.display = "none";
    cancelClicked();
    var reason = document.getElementById("reason").innerHTML;
    //update state + send message
    doPost({ "state": "Declined", "orderId": orderId, "reason": reason, "email": email });
}


function finishOrder(id, ordId) {
    var item = document.getElementById("item_" + id);
    item.style.display = "none";
    var email = document.getElementById("email_" + id).children[0].innerHTML;
    //update state
    doPost({ "state": "Finished", "orderId": ordId, "email":email });
}



var connection = new signalR.HubConnectionBuilder().withUrl("/orderHub").build();


connection.on("ReceiveMessage", function (obj) {
    obj = JSON.parse(obj);
    console.log(obj);
    var no_ords = document.getElementById("no_orders");
    if (no_ords != null)
        no_ords.style.display = "none";
    var row = document.getElementById("actorders");
    var count = row.childElementCount; count++;
    var total = 0;
    for (var i = 0; i < obj.ProductPrice.length; i++) {
        total += obj.ProductPrice[i] * obj.ProductQuantity[i];
    }
    var card = document.createElement("div");
    card.setAttribute("id", "item_" + count);
    card.setAttribute("class", "col-md-4");
    var tr = ``;
    for (var i = 0; i < obj.ProductName.length; i++) {
        tr += `<tr>`;
        tr += `<td>${obj.ProductName[i]}</td>`;
        tr += `<td>${obj.ProductQuantity[i]}</td>`;
        tr += `<td>${obj.ProductPrice[i]}</td>`;
        tr += `</tr>`
    }
    const options = { year: '2-digit', month: 'short', day: 'numeric', hour: 'numeric', minute: 'numeric', second: 'numeric', hour12: true };
    var date = new Date(obj.OrderPlaced);
    date = date.toLocaleTimeString("en-GB", options).replace(/ /g, '-').replace(",-", " ");
    date = date.slice(0, date.length - 3) + " " + date.slice(-2).toUpperCase();
    try {
        var html = `<div class="card border-primary mb-4">
        <div class="card-header" > Ordered on: ${date} </div>
            <div id="card_${count}" class="card-body">
                <p id="order_${count}">Order ${obj.State}</p>
                <p>Ordered by  ${obj.Name}</p>
                <p id="email_${count}"> <a href ="mailto:${obj.Email}">${obj.Email}</a></p>
                <p> ${obj.PhoneNumber}</p>
                <p>Payment method ${obj.Payment}</p>
                 <table class="table" style="table-layout: fixed; width: 100%;">
                        <thead>
                        <tr>
                            <th>Name</th>
                            <th>Count</th>
                            <th>Price</th>
                        </tr>
                        </thead>
                        <tbody>`
                         + tr +
                       ` </tbody>
                        <tfoot>
                        <tr>
                            <th id="total" colspan="2">Total :</th>
                            <td colspan="2">${total} Lei</td>
                        </tr>
                        </tfoot>
                    </table>
                    <button id="accept_${count}" onclick="orderAccepted(${count},${obj.Id})" type="button" class="btn btn-success">Accept</button>
                    <button id="decline_${count}" onclick="orderDenied(${count},${obj.Id})" type="button" class="btn btn-danger">Decline</button>
                    </div>`
    }
    catch (error) {
        colosle.log(error);
    }
    console.log(html);
    card.innerHTML = html;

    row.appendChild(card);

});

async function start() {
    try {
        await connection.start();
        console.log("connected");
    } catch (err) {
        console.log(err);
        setTimeout(() => start(), 5000);
    }
};
connection.onclose(async () => {
    await start();
});

start();
