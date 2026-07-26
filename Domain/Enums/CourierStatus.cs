namespace Domain.Enums;

public enum CourierStatus
{
    Offline,   //ғайрифаъол аст
    Available, //онлайн ва озод аст
    Assigned,  //Фармоиш ба курьер таъин шудааст
    PickingUp, //Курьер рафта истодааст, ки маҳсулотро гирад
    Delivering //Курьер маҳсулотро гирифта, ба муштарӣ мебарад
}