[Route("api/v1/search")]
[HttpPost]
Task<SearchResponse> SearchAsync(SearchRequest request, CancellationToken cancellationToken)
{
    // Подготовка запроса
    ProviderOneSearchRequest searchRequest =
        new ProviderOneSearchRequest(
            From = request.Origin,
            To = request.Destination,
            DateFrom = request.OriginDateTime,
            DateTo = request.Filters.DestinationDateTime,
            MaxPrice = request.Filters.MaxPrice,
        );

    // Получаем список всех маршрутов
    ProviderOneSearchResponse providerOneSearchResponse = getRoutes(cancellationToken);
    // фильтрация
    filteredRoutes = providerOneSearchResponse.Routes.Where(r => 
        r.From == searchRequest.From &&
        r.To == searchRequest.To &&
        r.DateFrom >= searchRequest.DateFrom &&
        r.DateTo <= searchRequest.DateTo &&
        r.Price <= searchRequest.MaxPrice
    );

    // Новая проекция для возврата и сохранения в кэше
    List<Route> returnList = filteredRoutes.Select(
        r => new Route(
            Id = Guid.NewGuid(/*универсальные данные*/),
            Origin = r.From,
            Destination = r.To,
            OriginDateTime = r.DateFrom,
            DestinationDateTime = r.DateTo,
            Price = r.Price,
            TimeLimit = r.TimeLimit)
    );

    SearchResponse searchResponse = new SearchResponse();
    searchResponse.Routes = returnList.ToArray();
    searchResponse.MinPrice = returnList.Select(r => r.Price).Min();
    searchResponse.MaxPrice = returnList.Select(r => r.Price).Max();
    searchResponse.MinMinutesRoute = returnList.select(r => r.DestinationDateTime - r.OriginDateTime).Min();
    searchResponse.MaxMinutesRoute = returnList.select(r => r.DestinationDateTime - r.OriginDateTime).Max();

    Cache.Write();

    return searchResponse;
}

[Route("api/v1/ping")]
[HttpGet]
Task<bool> IsAvailableAsync(CancellationToken cancellationToken)
{
    try
    {
        CheckStatus(cancellationToken);
    }
    catch (Exception)
    {
        return StatusCode(500);
    }
    return StatusCode(200);
}
