// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

$("#input_search").keypress(function (e)
{
    if (e.keyCode == 13) {
    e.preventDefault();
$("#btn_search").click();
    }
});


function search()
{
    search_text = $("#input_search").val()
    //when nothing is written in input_search
    if (search_text == '') {
        alert("Invalid search text!");
        $(".jumbotron").hide();
        return;
    }
    only_photos = $('#only_photos').is(":checked")
    $.ajax({
        type: "GET",
        url: "/api/search?query=" + search_text + "&only_photos=" + only_photos,
        success: function (response) {
            $(".jumbotron").show();
            $(".result").html("");
            parse_json(response);
            },
        error: function (response) {
            console.log('error');
                }
            });
};


function load_more(pagination_token = null)
{
    search_text = $("#input_search").val()
    only_photos = $('#only_photos').is(":checked")
    if (pagination_token != null)
        pag_url = "&pagination_token=" + pagination_token;
    else
        pag_url = "";

    $.ajax({
        type: "GET",
        url: "/api/search?query=" + search_text + "&only_photos=" + only_photos + pag_url,
        success: function (response) {
            parse_json(response);
                },
        error: function (response) {
            console.log('error');
                }
    });
};

function parse_json(response) {
    html_data = "";
    returnedData = JSON.parse(response);
    if (returnedData.tweets == null)
    {
        $(".result").append("<b>No data found!<b>");
        return;
    }
    $.each(returnedData.tweets, function (index, tweet)
    {
        html_data = html_data + "<p>" + convertLinks(tweet.text) + "</p>"
        if (tweet.images) {
            $.each(tweet.images, function (index_image, image)
            {
                if (image.url != undefined)
                    image_url = image.url
                else
                    image_url = image.preview_image_url

                 if (image_url != undefined)
                    html_data = html_data + "<img src='" + image_url + "' class='img-fluid' style='max-height:400px;' alt='Image' />"
            });
        }
        html_data = html_data + "<hr class='my-4'>";
    });

    $(".result").append(html_data);
    $("#load_more").remove();
    if (returnedData.next_token)
    $(".result").append("<div id='load_more' class='col text-center'><input class='btn btn-info my-2 my-sm-0' type='button' value='Load more' onclick='load_more(\""+returnedData.next_token+"\")'></div>");
}

function convertLinks(input)
{
    let text = input.replace(/\n/g, "<br />");
    const linksFound = text.match(/(?:www|https?)[^\s]+/g);
    const aLink = [];

    if (linksFound != null)
    {
        for (let i = 0; i < linksFound.length; i++)
        {
            let replace = linksFound[i];
            if (!(linksFound[i].match(/(http(s?)):\/\//))) {replace = 'http://' + linksFound[i]}
            let linkText = replace;
            if (linkText.match(/youtu/))
            {
                let youtubeID = replace.split('/').slice(-1)[0];
                aLink.push('<div class="video-wrapper"><iframe src="https://www.youtube.com/embed/' + youtubeID + '" frameborder="0" allow="accelerometer; autoplay; encrypted-media; gyroscope; picture-in-picture" allowfullscreen></iframe></div>')
            }
            else if (linkText.match(/vimeo/))
            {
                let vimeoID = replace.split('/').slice(-1)[0];
                aLink.push('<div class="video-wrapper"><iframe src="https://player.vimeo.com/video/' + vimeoID + '" frameborder="0" webkitallowfullscreen mozallowfullscreen allowfullscreen></iframe></div>')
            }
            else
            {
                aLink.push('<a href="' + replace + '" target="_blank">' + linkText + '</a>');
            }
                text = text.split(linksFound[i]).map(item => { return aLink[i].includes('iframe') ? item.trim() : item }).join(aLink[i]);
            }
        return text;
    }
    else
    {
        return input;
    }
}

