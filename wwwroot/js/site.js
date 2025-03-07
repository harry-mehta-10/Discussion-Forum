(function () {

    // Function to load the comments for a specific discussion
    function loadComments(discussionId) {
        $.ajax({
            url: `/api/CommentsApi/GetDiscussionComments/${discussionId}`,
            method: 'GET',
            success: function (comments) {
                const commentsContainer = $('#comments-container');
                commentsContainer.empty();

                if (comments.length === 0) {
                    commentsContainer.append('<p class="text-muted">No comments yet. Be the first to comment!</p>');
                    return;
                }

                // Loops through and display each comment
                comments.forEach(comment => {
                    commentsContainer.append(`
                        <div class="comment border-bottom p-3">
                            <p class="mb-1">${comment.content}</p>
                            <small class="text-muted">
                                Posted by ${comment.author} on ${comment.createDate}
                            </small>
                        </div>
                    `);
                });
            },
            error: function (xhr, status, error) {
                console.error('Error loading comments:', error);
                $('#comments-container').html(
                    '<div class="alert alert-danger">Error loading comments. Please try refreshing the page.</div>'
                );
            }
        });
    }

    // Initializes the comment form and load existing comments if any
    function initializeCommentForm() {
        const discussionId = $('#DiscussionId').val();
        if (discussionId) {
            loadComments(discussionId); // Fetch comments when the page loads
        }

        // Handles form submission for posting a new comment
        $('#comment-form').off('submit').on('submit', function (e) {
            e.preventDefault();

            const submitButton = $(this).find('button[type="submit"]');
            if (submitButton.prop('disabled')) return;  // Prevent multiple submissions

            const content = $('#Content').val()?.trim() || '';
            const author = $('#Author').val()?.trim() || '';
            const discussionId = parseInt($('#DiscussionId').val());

            if (!content) {
                $('#Content').addClass('is-invalid');
                return; 
            }

            $('#Content').removeClass('is-invalid');
            submitButton.prop('disabled', true)
                .html('<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Posting...');

            const data = {
                content: content,
                discussionId: discussionId,
                author: author
            };

            $.ajax({
                url: '/api/CommentsApi/Create',
                method: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(data),
                success: function (response) {
                    if (response.success) {
                        $('#Content').val('');  
                        $('#Author').val('');
                        loadComments(discussionId);  
                    } else {
                        alert('Error: ' + response.message);
                    }
                },
                error: function (xhr, status, error) {
                    console.error('Error posting comment. Details:', xhr.responseText);
                    alert('Error posting comment. Please try again.');
                },
                complete: function () {
                    submitButton.prop('disabled', false).text('Post Comment');
                }
            });
        });
    }

    // Calls the function to initialize the comment form when the page is ready
    $(document).ready(initializeCommentForm);

})();
