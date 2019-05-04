const markdown = require('simple-markdown');
import hljs from 'highlight.js/lib/highlight';

['cs', 'json'].forEach((langName) => {
    // Using require() here because import() support hasn't landed in Webpack yet
    const langModule = require(`highlight.js/lib/languages/${langName}`);
    hljs.registerLanguage(langName, langModule);
});

function htmlTag(tagName, content, attributes, isClosed = true, state = { }) {
	if (typeof isClosed === 'object') {
		state = isClosed;
		isClosed = true;
	}

	if (!attributes)
		attributes = { };

	if (attributes.class && state.cssModuleNames)
		attributes.class = attributes.class.split(' ').map(cl => state.cssModuleNames[cl] || cl).join(' ');

	let attributeString = '';
	for (let attr in attributes) {
		// Removes falsy attributes
		if (Object.prototype.hasOwnProperty.call(attributes, attr) && attributes[attr])
			attributeString += ' ' + attr + '="' + attributes[attr] + '"';
	}

	let unclosedTag = '<' + tagName + attributeString + '>';

	if (isClosed)
		return unclosedTag + content + '</' + tagName + '>';
	return unclosedTag;
}
markdown.htmlTag = htmlTag;

const rules = {
	codeBlock: Object.assign({ }, markdown.defaultRules.codeBlock, {
		html: (node, output, state) => {
			let code;
			if (node.lang && hljs.getLanguage(node.lang))
				code = hljs.highlight(node.lang, node.content, true); // Discord seems to set ignore ignoreIllegals: true

			if (code && state.cssModuleNames) // Replace classes in hljs output
				code.value = code.value.replace(/<span class="([a-z0-9-_ ]+)">/gi, (str, m) =>
					str.replace(m, m.split(' ').map(cl => state.cssModuleNames[cl] || cl).join(' ')));

			return htmlTag('pre', htmlTag(
				'code', code ? code.value : node.content, { class: `hljs${code ? ' ' + code.language : ''}` }, state
			), null, state);
		}
	}),
	fence: Object.assign({ }, markdown.defaultRules.fence, {
		match: markdown.inlineRegex(/^ *(`{3,}) *(\S+)? *\n([\s\S]+?)\s*\1 *(?:\n *)*/)
	}),
	newline: markdown.defaultRules.newline,
	escape: markdown.defaultRules.escape,
	autolink: Object.assign({ }, markdown.defaultRules.autolink, {
		parse: capture => {
			return {
				content: [{
					type: 'text',
					content: capture[1]
				}],
				target: capture[1]
			};
		},
		html: (node, output, state) => {
			return htmlTag('a', output(node.content, state), { href: markdown.sanitizeUrl(node.target) }, state);
		}
	}),
	url: Object.assign({ }, markdown.defaultRules.url, {
		parse: capture => {
			return {
				content: [{
					type: 'text',
					content: capture[1]
				}],
				target: capture[1]
			}
		},
		html: (node, output, state) => {
			return htmlTag('a', output(node.content, state), { href: markdown.sanitizeUrl(node.target) }, state);
		}
	}),
	em: markdown.defaultRules.em,
	strong: markdown.defaultRules.strong,
	u: markdown.defaultRules.u,
	del: Object.assign({ }, markdown.defaultRules.del, {
		match: markdown.inlineRegex(/^~~(\s*?(?:\\[\s\S]|~(?!~)|[^\s\\~]|\s+(?!~~))+?\s*?)~~/),
	}),
	inlineCode: markdown.defaultRules.inlineCode,
	text: Object.assign({ }, markdown.defaultRules.text, {
		match: source => /^[\s\S]+?(?=[^0-9A-Za-z\s\u00c0-\uffff-]|\n\n|\n|\w+:\S|$)/.exec(source),
		html: function(node, output, state) {
			if (state.escapeHTML)
				return markdown.sanitizeText(node.content);

			return node.content;
		}
	}),
	specialCaseArms: {
		order: markdown.defaultRules.escape.order - 0.5,
		match: source => /^¯\\_\(ツ\)_\/¯/.exec(source),
		parse: function(capture, parse, state) {
			return {
				content: parse(capture[0].replace(/^¯\\_\(ツ\)_\/¯/, '¯\\\\\\_(ツ)_/¯'), state)
			};
		},
		html: function(node, output, state) {
			return output(node.content, state);
		},
	},
	br: Object.assign({ }, markdown.defaultRules.br, {
		match: markdown.anyScopeRegex(/^\n/),
	}),
	spoiler: {
		order: 0,
		match: source => /^\|\|((?:.|\n)+?)\|\|/.exec(source),
		parse: function(capture, parse, state) {
			return {
				content: parse(capture[1], state)
			};
		},
		html: function(node, output, state) {
			return htmlTag('span', output(node.content, state), { class: 'd-spoiler' }, state);
		}
	}
};

const discordCallbackDefaults = {
	user: node => '@' + node.id,
	channel: node => '#' + node.id,
	role: node => '&' + node.id,
	emoji: node => ':' + markdown.sanitizeText(node.name) + ':',
	everyone: () => '@everyone',
	here: () => '@here'
};

let discordCallback = discordCallbackDefaults;

const rulesDiscord = {
	discordUser: {
		order: markdown.defaultRules.strong.order,
		match: source => /^<@!?([0-9]*)>/.exec(source),
		parse: function(capture) {
			return {
				id: capture[1]
			};
		},
		html: function(node, output, state) {
			return htmlTag('span', discordCallback.user(node), { class: 'd-mention d-user' }, state);
		}
	},
	discordChannel: {
		order: markdown.defaultRules.strong.order,
		match: source => /^<#?([0-9]*)>/.exec(source),
		parse: function(capture) {
			return {
				id: capture[1]
			};
		},
		html: function(node, output, state) {
			return htmlTag('span', discordCallback.channel(node), { class: 'd-mention d-channel' }, state);
		}
	},
	discordRole: {
		order: markdown.defaultRules.strong.order,
		match: source => /^<@&([0-9]*)>/.exec(source),
		parse: function(capture) {
			return {
				id: capture[1]
			};
		},
		html: function(node, output, state) {
			return htmlTag('span', discordCallback.role(node), { class: 'd-mention d-role' }, state);
		}
	},
	discordEmoji: {
		order: markdown.defaultRules.strong.order,
		match: source => /^<(a?):(\w+):([0-9]*)>/.exec(source),
		parse: function(capture) {
			return {
				animated: capture[1] === "a",
				name: capture[2],
				id: capture[3],
			};
		},
		html: function(node, output, state) {
			return htmlTag('span', discordCallback.emoji(node), { class: `d-emoji${node.animated ? ' d-emoji-animated' : ''}` }, state);
		}
	},
	discordEveryone: {
		order: markdown.defaultRules.strong.order,
		match: source => /^@everyone/.exec(source),
		parse: function() {
			return { };
		},
		html: function(node, output, state) {
			return htmlTag('span', discordCallback.everyone(node), { class: 'd-mention d-user' }, state);
		}
	},
	discordHere: {
		order: markdown.defaultRules.strong.order,
		match: source => /^@here/.exec(source),
		parse: function() {
			return { };
		},
		html: function(node, output, state) {
			return htmlTag('span', discordCallback.here(node), { class: 'd-mention d-user' }, state);
		}
	}
};
Object.assign(rules, rulesDiscord);

const rulesDiscordOnly = Object.assign({ }, rulesDiscord, {
	text: Object.assign({ }, markdown.defaultRules.text, {
		match: source => /^[\s\S]+?(?=[^0-9A-Za-z\s\u00c0-\uffff-]|\n\n|\n|\w+:\S|$)/.exec(source),
		html: function(node, output, state) {
			if (state.escapeHTML)
				return markdown.sanitizeText(node.content);

			return node.content;
		}
	})
});


const rulesEmbed = Object.assign({ }, rules, {
	link: markdown.defaultRules.link
});

const parser = markdown.parserFor(rules);
const htmlOutput = markdown.htmlFor(markdown.ruleOutput(rules, 'html'));
const parserDiscord = markdown.parserFor(rulesDiscordOnly);
const htmlOutputDiscord = markdown.htmlFor(markdown.ruleOutput(rulesDiscordOnly, 'html'));
const parserEmbed = markdown.parserFor(rulesEmbed);
const htmlOutputEmbed = markdown.htmlFor(markdown.ruleOutput(rulesEmbed, 'html'));

/**
 * Parse markdown and return the HTML output
 * @param {String} source Source markdown content
 * @param {Object} [options] Options for the parser
 * @param {Boolean} [options.embed=false] Parse as embed content
 * @param {Boolean} [options.escapeHTML=true] Escape HTML in the output
 * @param {Boolean} [options.discordOnly=false] Only parse Discord-specific stuff (such as mentions)
 * @param {Object} [options.discordCallback] Provide custom handling for mentions and emojis
 * @param {Object} [options.cssModuleNames] An object mapping css classes to css module classes
 */
export default function toHTML(source, options) {
	options = Object.assign({
		embed: false,
		escapeHTML: true,
		discordOnly: false,
		discordCallback: { }
	}, options || { });

	let _parser = parser;
	let _htmlOutput = htmlOutput;
	if (options.discordOnly) {
		_parser = parserDiscord;
		_htmlOutput = htmlOutputDiscord;
	} else if (options.embed) {
		_parser = parserEmbed;
		_htmlOutput = htmlOutputEmbed;
	}

	// TODO: Move into state
	discordCallback = Object.assign({ }, discordCallbackDefaults, options.discordCallback);

	const state = {
		inline: true,
		escapeHTML: options.escapeHTML,
		cssModuleNames: options.cssModuleNames || null
	};

	return _htmlOutput(_parser(source, state), state);
}