

google.charts.load('current', { 'packages': ['corechart'] });
google.charts.setOnLoadCallback(getChartData());

/* -----------------------------
    CODE FOR REGISTER PAGE
 --------------------------------- */

function registerOnSubmit() {
    // let user = $("#user").val();
    let uid = $("#uid").val();
    let uni = $("#uni").val();
    let fname = $("#fname").val();
    let lname = $("#lname").val();
    let pass = $("#pass").val();
    let confirm = $("#confirm").val();
    let major = $("#major").val();
    let responses = [uid, uni, fname, lname, pass, confirm, major];

    for (var i = 0; i < responses.length; i++) {
        if (pass != confirm) {
            alert("Passwords don't match.");
            break;
        }

        if (responses[i] == "") {
            alert("Please complete all fields.");
            break;
        } else {
            insertSignUpData(responses);
            alert("Account succesfully created!");
            window.location.href = "index.html"
            break;
        }
    }
}

// Put data into database
function insertSignUpData(responses) {
    let userData = {
        "user_ID":responses[0],
        "University_name": responses[1],
        "Fname": responses[2],
        "Lname": responses[3],
        "Password": responses[4],
        "Major": responses[6],
        "AdminStatus": 0
    };

    console.log(JSON.stringify(userData));

    // Call C# metohd to insert into table
    $.ajax({
        type: 'POST',
        url: "AccountServices.asmx/RequestAccount",
        data: JSON.stringify(userData),
        contentType: "application/json; charset=utf-8",
        dataType: 'json',
        success: function(msg) {
            console.log("success");
        }
    });

}

/* 
    CODE FOR GRADE ENTRY PAGE
*/

function submitGradeEntryData() {
    let year = $("#year").val();
    let term = $("#term").val();
    let totalCredits = $("#totalCredits").val();
    let currentGpa = $("#currentGpa").val();
    let gradeResponses = [year, term, totalCredits, currentGpa];

    for (var i = 0; i < gradeResponses.length; i++) {
        if (gradeResponses[i] == "") {
            alert("Please complete all fields!");
            break;
        } else if (gradeResponses[3] > 4) {
            alert("GPA cannot be over 4.0.");
            break;
        } else {
            insertGradeEntryData(gradeResponses);
            break;
        }
    }      
}

function insertGradeEntryData(grades) {
    let gradeResponses = {
        "Year": grades[0],
        "Term": grades[1],
        "Total_Credits": grades[2],
        "CurrentGPA": grades[3]
    };

    $.ajax({
        type: 'POST',
        url: "AccountServices.asmx/HandleGradeEntryData",
        data: JSON.stringify(gradeResponses),
        contentType: "application/json; charset=utf-8",
        dataType: 'json',
        success: function (msg) {
            console.log("success");
        }
    });
}

// GetAccounts js and ajax call
function getChartData() {
    $.ajax({
        url: "AccountServices.asmx/GetAccounts",
        type: "POST",
        dataType: "json",
        contextType: "application/json; charset=utf-8",
        data: { action: "getChartData" },
        traditional: true,
        success: function (results) {
            var data = google.visualization.arrayToDataTable(grades);
            var options = {
                title: 'My Daily Activities'
            };
            var chart = new google.visualization.PieChart(document.getElementById('piechart'));
            chart.draw(data, options);

            console.log(data);
            
        },
        error: function (jqXHR, textStatus, errorThrown) {
            console.log('Could not get posts, server response: ' + textStatus + ': ' + errorThrown);
        }
    }).responseJSON;
}


