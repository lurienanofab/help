(function ($) {

    $.fn.helpdesk = function (options) {
        return this.each(function () {
            var $this = $(this);

            var table = null;

            var opt = $.extend({ "apiurl": "api" }, $this.data(), options);

            var apiUrl = function () {
                return opt.apiurl || "api";
            }

            var toggleOtherTextBox = function (sender) {
                var parent = sender.closest("div");
                if (sender.val() == "other") {
                    parent.find(".other-textbox-container")
                        .show()
                        .find("input")
                        .focus();
                }
                else
                    parent.find(".other-textbox-container").hide();
            }

            var getTopic = function () {
                var selectedValue = $(".topic-select", $this).val();
                var selectedText = $(".topic-select option:selected", $this).text();
                var result = (selectedValue == "other") ? $(".other-topic-textbox", $this).val() : selectedText;
                return result;
            }

            var getLocation = function () {
                var selectedValue = $(".location-select", $this).val();
                var selectedText = $(".location-select option:selected", $this).text();
                var result = (selectedValue == "other") ? $(".other-location-textbox", $this).val() : selectedText;
                return result;
            }

            var getSubject = function () {
                return $(".subject-textbox", $this).val();
            }

            var getMessage = function () {
                return $(".message-textarea", $this).val();
            }

            var getEmail = function () {
                return opt.email;
            }

            var getName = function () {
                return opt.name;
            }

            var getQueue = function () {
                return opt.queue;
            }

            var getPri = function () {
                return opt.priority;
            }

            var validate = function () {
                var result = true;
                var notify = $(".notify", $this);
                notify.html($("<div/>", { "class": "alert alert-danger", "role": "alert" }));
                if (getTopic() == "") {
                    $(".alert", notify).append($("<div/>").html("&bull; Please enter a topic."));
                    result = false;
                }
                if (getLocation() == "") {
                    $(".alert", notify).append($("<div/>").html("&bull; Please enter a location."));
                    result = false;
                }
                if (getSubject() == "") {
                    $(".alert", notify).append($("<div/>").html("&bull; Please enter a description."));
                    result = false;
                }
                if (getMessage() == "") {
                    $(".alert", notify).append($("<div/>").html("&bull; Please enter an issue."));
                    result = false;
                }
                return result;
            }

            var getNotification = function (data) {
                if (data.error)
                    return $("<div/>", { "class": "alert alert-danger", "role": "alert" }).html(data.Message);
                else
                    return $("<div/>", { "class": "alert alert-success", "role": "alert" }).html("Ticket created.");
            }

            var showDialog = function (ticketID) {
                $(".ticket-detail", $this).ticket({
                    "apiurl": apiUrl(),
                    "ticketID": ticketID,
                    "source": "Help",
                    "onload": function (ticket) {
                        //$(".ticket-detail", $this).prepend(
                        //    $("<div/>")
                        //        .css({ "padding": "0 0 10px 10px" })
                        //        .append("[")
                        //        .append(
                        //            $("<a/>", { "href": "#" }).html("hide").on("click", function (e) {
                        //                e.preventDefault();
                        //                $(".ticket-detail", $this).html($("<em/>").css("color", "#808080").html("Click a Ticket # to view detail"));
                        //            }))
                        //        .append("]")
                        //);
                    }
                });
            }

            var getTickets = function (email) {
                return $.ajax({
                    url: apiUrl(),
                    data: { "command": "select-tickets-by-email", "email": email },
                    type: "POST",
                    dataType: "json"
                });
            }

            var loadTickets = function (arg) {
                table.api().ajax.reload();
            }

            var resetAddTicketForm = function () {
                $(".topic-select", $this).prop("selectedIndex", 0);
                $(".location-select", $this).prop("selectedIndex", 0);
                $(".other-textbox-container", $this).hide();
                $(".other-topic-textbox", $this).val("");
                $(".other-location-textbox", $this).val("");
                $(".subject-textbox", $this).val("");
                $(".message-textarea", $this).val("");
            }

            var addTicket = function (callback) {
                if (typeof callback != "function")
                    callback = function () { console.log("addTicket"); };

                var notify = $(".notify", $this);
                notify.html("");

                $(".create-ticket-button").prop("disabled", true);
                $(".create-ticket-working", $this).show();

                var message = getMessage();
                var email = getEmail();
                var name = getName();
                var queue = getQueue();
                var pri = getPri();
                message = "Topic: " + getTopic() + "\r\nLocation: " + getLocation() + "\r\n--------------------\r\n" + message;
                $.ajax({
                    "url": apiUrl(),
                    "type": "POST",
                    "data": { "command": "add-ticket", "email": email, "name": name, "queue": queue, "subject": getSubject(), "message": message, "pri": pri, "search": "by-email" },
                    "dataType": "json",
                    "success": function (data, textStatus, jqXHR) {
                        notify.html(getNotification(data));
                        loadTickets(data);
                        resetAddTicketForm();
                    },
                    "error": function (jqXHR, textStatus, errorThrown) {
                        notify.html($("<div/>", { "class": "alert alert-danger", "role": "alert" }).html(errorThrown));
                    },
                    "complete": function () {
                        $(".create-ticket-button").prop("disabled", false);
                        $(".create-ticket-working", $this).hide();
                        callback();
                    }
                });
            }

            var getSearchEmail = function () {
                return $(".email-search-textbox", $this).val();
            }

            console.log($(".ticket-table", $this));
            table = $(".ticket-table", $this).dataTable({
                "columns": [
                   {
                       "data": "ticketID", "className": "ticket-id", "render": function (data, type, row, meta) {
                           return '<a href="#" class="view-ticket-link">' + data + '</a>';
                       }
                   },
                   { "data": "email", "className": "email" },
                   { "data": "subject", "className": "description" },
                   {
                       "data": "resource_id", "className": "resource", "render": function (data, type, row, meta) {
                           return data == 0 ? "" : data;
                       }
                   },
                   {
                       "data": "created", "className": "created-on", "render": function (data, type, row, meta) {
                           return moment(data).format("M/D/YYYY h:MM A");
                       }
                   },
                   { "data": "assigned_to", "className": "assigned-to" }
                ],
                "order": [[4, "desc"]],
                "processing": true,
                "language": { "zeroRecords": "No tickets were found." },
                "ajax": function (data, callback, settings) {
                    $(".message", $this).html("");
                    getTickets(getSearchEmail()).done(function (d, textStatus, jqXHR) {
                        callback({ "data": d.tickets });
                    }).fail(function (jqXHR, textStatus, errorThrown) {
                        var errmsg = (jqXHR.responseJSON && jqXHR.responseJSON.message) ? jqXHR.responseJSON.message : errorThrown;
                        $(".message", $this).html(
                            $("<div/>", { "class": "alert alert-danger", "role": "alert" }).html(errmsg)
                        );
                    });
                }
            });

            $this.on("change", ".topic-select, .location-select", function (e) {
                toggleOtherTextBox($(this));
            }).on("click", ".view-ticket-link", function (e) {
                e.preventDefault();
                showDialog($(this).text());
            }).on("click", ".create-ticket-button", function (e) {
                if (validate()) {
                    addTicket(function () { });
                }
            }).on("click", ".email-search-button", function (e) {
                $(".ticket-detail .container-fluid", $this).hide();
                $(".ticket-detail .detail-message", $this).html($("<em/>").css("color", "#808080").html("Click a Ticket # to view detail."));
                var email = getSearchEmail();
                if (email != "") {
                    $(".search-email", $this).html(email);
                    loadTickets(email);
                }
            }).on("loadTickets", function (e, email) {
                $(".email-search-textbox", $this).val(email)
                loadTickets(email);
            });
        });
    }
}(jQuery));