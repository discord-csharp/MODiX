# Host Process

The MODiX "Host Process" component is the "glue" of the MODiX Application. It's responsible for controlling assembly and configuration of the various services and components that make up the application as a whole. It is, essentially, the entry-point of the application.

## Purpose

1. Define the entry point of the application
2. Configure the Dependency Injection container
3. Configure and host application services and components
4. Configure and host development and design tools

## Methodology

### Modular Setup

The "Host Process" should not be concerned with configuring all services and components directly. Rather, It should invoke high-level methods for different modules of the application, allowing modules to configure themselves and the services and components they define. This helps reduce visual clutter. E.G.

```cs
public static IServiceCollection AddModixPromotions(this IServiceCollection services)
    => services
        .AddScoped<IPromotionsService, PromotionsService>()
        .AddScoped<IPromotionActionRepository, PromotionActionRepository>()
        .AddScoped<IPromotionCampaignRepository, PromotionCampaignRepository>()
        .AddScoped<IPromotionCommentRepository, PromotionCommentRepository>();
```

### Fluent syntax

Fluent syntax should be used whenever feasible, to highlight the intended declarative nature of application configuration. E.G.

```cs
services
    .AddModixCore()
    .AddModixMessaging()
    .AddModixModeration()
    .AddModixPromotions()
    .AddCodePaste()
    .AddCommandHelp()
    .AddGuildStats()
    .AddModixTags()
    .AddStarboard()
    .AddAutoRemoveMessage()
    .AddEmojiStats()
    .AddImages()
    .AddGiveaways();
```
