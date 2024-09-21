function goBackPage(date = null) {

    if (date == null) {
        window.history.back();
    } else {
        var partes = date.split("/");

        var month = partes[1];
        var year = partes[2];
        window.location.href = "/History/GetBillIncomeAndMonthlyTotal?month=" + month + "&year=" + year;
    }
}