import { ChangeDetectorRef, Component, OnInit, ViewEncapsulation } from '@angular/core';
import { FormControl, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService } from 'app/core/auth/auth.service';
import { LikeService } from 'app/core/like/like.service';
import { RetweetService } from 'app/core/retweet/retweet.service';
import { TweetViewModel } from 'app/modules/models/tweet-model';
import { ToastrService } from "ngx-toastr";
import { CommentService } from '../../../core/comment/comment.service';
import { GlobalNotificationService } from '../../../core/notification/notification.service';
import { TweetService } from '../../../core/tweet/tweet.service';
import { CommentViewModel } from '../../models/comment-model';
import { UserViewModel } from '../user.model';
import { UserService } from '../user.service';

@Component({
	selector: 'home',
	templateUrl: './home.component.html',
	styles: ['.likeColor { color: #F91880; }', '.retweetColor { color:#2ecc71; }'],
	encapsulation: ViewEncapsulation.None,
})
export class HomeComponent implements OnInit {
	show: boolean = false;
	postId: string;
	CommentOpen: boolean = false;
	commentPageNumber = 1;
	pageNumber = 1;
	commentVMList: CommentViewModel[];
	commentForm = new FormControl('', Validators.required);
	infiniteScrollStatus = false;
	userData: UserViewModel;
	tweetVMlist: TweetViewModel[];
	tweetForm = new FormControl('', Validators.required);

	tweetId: string;
	userId: string;
	isSingleTweetPage: boolean = false;

	constructor(
		private commentService: CommentService,
		public tweetService: TweetService,
		private cdr: ChangeDetectorRef,
		private likeService: LikeService,
		private retweetService: RetweetService,
		private userService: UserService,
		private authService: AuthService,
		private notificationService: GlobalNotificationService,
		private toastService: ToastrService,
		private router: Router,
		private activatedRoute: ActivatedRoute) {

		let tweetId = this.activatedRoute.snapshot.paramMap.get('tweetId');
		let userId = this.activatedRoute.snapshot.paramMap.get('userId');
		if (!!tweetId && userId) {
			this.isSingleTweetPage = true;
			this.tweetId = tweetId.toString();
			this.userId = userId.toString();
		}
	}

	ngOnInit(): void {
		if (this.isSingleTweetPage) {
			this.tweetService.getTweetById(this.tweetId, this.userId).subscribe({
				next: res => {
					this.tweetVMlist = [];

					this.tweetVMlist.push(res);
				},
				error: err => {
					console.log(err);
				}
			});
		}
		else {
			// this.comment = new FormControl('', validator.required);
			this.userService.getUserById(this.authService.userId).subscribe({
				next: res => {
					this.userData = res;
				},
				error: err => {
					console.log(err);
				}
			});
			this.tweetService.getHomeTimelineData(this.pageNumber).subscribe((response) => {
				if (response) {
					this.tweetVMlist = response;
					// console.log(this.pageNumber);
					// console.log(response)
				}
			},
				(err) => {
					console.log(err);
				});

		}

	}

	onRedirectToProfile(userId: string): void {
		this.router.navigate(['/profile/', userId]);
	}

	showMessage() {
		this.toastService.success('', 'New Notification');
	}

	viewMoreComment(tweet: TweetViewModel): void {
		this.commentService.getComments(tweet.id, ++this.commentPageNumber).subscribe({
			next: (comments: CommentViewModel[]) => {
				this.commentVMList.push(...comments);
			},
			error: (err) => {
				console.log(err);
			}
		});
	}

	createComment(tweet: TweetViewModel): void {

		const payload = {
			tweetId: tweet.id,
			tweetCreatorId: tweet.tweetCreatorId,
			content: this.commentForm.value
		};

		this.commentService.createComment(payload).subscribe({

			next: (response) => {
				this.commentForm.reset();
				const index = this.tweetVMlist.indexOf(tweet);

				this.tweetVMlist[index].comments = response.totalComments;
				this.notificationService.sendComment(tweet.id, tweet.tweetCreatorId);

				this.commentService.getComments(tweet.id, 1).subscribe({
					next: (res) => {
						this.commentVMList = res;
					},
					error: (err) => {
						console.log(err);
					}
				});
			},
			error: (err) => {
				console.log(err);
			}
		});
	}

	getComments(tweet: TweetViewModel): void {
		this.postId = tweet.id;
		if (this.CommentOpen === true) {
			this.CommentOpen = false;
		}
		this.CommentOpen = !this.CommentOpen;
		this.commentService.getComments(tweet.id, 1).subscribe({
			next: (res) => {
				this.commentVMList = res;
				this.commentPageNumber = 1;
			},
			error: (err) => {
				console.log(err);
			}
		});
	}

	likeChange(tweet: TweetViewModel) {

		this.likeService.likeTweet(tweet.id, tweet.tweetCreatorId).subscribe({
			next: response => {
				let index = this.tweetVMlist.indexOf(tweet);

				this.tweetVMlist[index].likes = response.likes;

				this.tweetVMlist[index].isLikedByCurrentUser = response.isLikedByCurrentUser;

				if (response.isLikedByCurrentUser) {
					this.notificationService.sendLike(tweet.id, tweet.tweetCreatorId);
				}
			},
			error: err => {
				console.log(err);
			}
		})
	}

	deleteComment(tweet: TweetViewModel, comment: CommentViewModel): void {
		this.commentService.deleteComment(tweet.id, comment.id).subscribe((response) => {
			const index = this.tweetVMlist.indexOf(tweet);
			this.tweetVMlist[index].comments = response.totalComments;
			const commentIndex = this.commentVMList.indexOf(comment);
			this.commentVMList.splice(commentIndex, 1);
		});
	}

	retweetChange(tweet: TweetViewModel) {

		this.retweetService.retweet(tweet.id, tweet.tweetCreatorId).subscribe({
			next: response => {
				let index = this.tweetVMlist.indexOf(tweet);
				this.tweetVMlist[index].retweets = response.retweets;
				this.tweetVMlist[index].isRetweetedByCurrentUser = response.isRetweetedByCurrentUser;

				if (response.isRetweetedByCurrentUser) {
					this.notificationService.sendRetweet(tweet.id, tweet.tweetCreatorId);
				}
			},
			error: err => {
				console.log(err);
			}
		})
	}


	onScroll(): void {
		this.tweetService
			.getHomeTimelineData(++this.pageNumber)
			.subscribe((tweets: TweetViewModel[]) => {
				if (tweets.length == 0) {
					this.infiniteScrollStatus = true
				}
				this.tweetVMlist.push(...tweets);
				// console.log(this.pageNumber);
				// console.log(tweets);
			});

	}

	postTweet(): void {

		this.tweetService.tweet.content = this.tweetForm.value;

		this.tweetService.createPost(this.tweetService.tweet).subscribe({
			next: (res) => {
				if (res) {
					this.tweetForm.reset();
					this.toastService.success("Post Created !!");
				}
			},
			error: (err) => {
				console.log(err);
			},
		});
	}

}
