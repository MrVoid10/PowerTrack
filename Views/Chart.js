<canvas id="energyChart"></canvas>

<script>
var ctx = document.getElementById('energyChart').getContext('2d');
var chart = new Chart(ctx, {
    type: 'line',
    data: {
        labels: @Html.Raw(Json.Serialize(Model.Select(m => m.Month))),
        datasets: [{
            label: 'Consum kWh',
            data: @Html.Raw(Json.Serialize(Model.Select(m => m.KWh)))
        }]
    }
});
</script>
